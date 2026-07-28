using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;


/*
 * Gestisce lo stato autoritativo della partita multiplayer.
 *
 * Il Master registra le morti, attribuisce le uccisioni,
 * determina il vincitore e sincronizza il risultato finale.
 */

[DisallowMultipleComponent]
[RequireComponent(typeof(PhotonView))]
public sealed class PhotonGameManager : MonoBehaviourPun
{
    public static PhotonGameManager Instance { get; private set; }

    [Header("Player HUD")]
    [SerializeField] private UIPlayerLives[] uiPlayerLives;

    [Header("Timer")]
    [SerializeField] private UITimer uiTimer;

    [Header("End Game")]
    [SerializeField] [Min(0f)] private float endPanelDelay = 1f;

    [Header("Multiplayer Result Panel")]
    [SerializeField] private MultiplayerResultPanelController multiplayerResultPanel;

    [Header("Rematch")]
    [SerializeField] private PhotonRematchManager photonRematchManager;

    private readonly List<PhotonPlayerController> alivePlayers = new();
    private readonly Dictionary<int, int> killCounts = new();

    private int localHumanViewId = -1;

    private bool finalBattleMusicStarted;
    private bool isMatchEnded;
    private bool isReplayStarting;


    // Inizializza il Singleton presente nella scena multiplayer.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    // Recupera eventuali giocatori inizializzati prima del Manager e avvia la musica relativa alla mappa
    private void Start()
    {
        Time.timeScale = 1f;

        PhotonPlayerController[] existingPlayers = FindObjectsByType<PhotonPlayerController>(FindObjectsSortMode.None);

        foreach (PhotonPlayerController player in existingPlayers)
        {
            RegisterPlayer(player);
        }

        if (AudioManager.Instance != null && MapManager.Instance != null)
        {
            AudioManager.Instance.PlayBackgroundMusic(MapManager.Instance.cuurEMap);
        }
    }



    // Registra un personaggio tra quelli ancora vivi
    // Su ogni Client collega il realtivo HUD, mentre soltatnto il Master ascolta l'evento autoritativo di morte
    public void RegisterPlayer(PhotonPlayerController player)
    {
        if (player == null || alivePlayers.Contains(player))
        {
            return;
        }

        alivePlayers.Add(player);

        BindPlayerToHUD(player);

        if (!killCounts.ContainsKey(player.ViewId))
        {
            killCounts.Add(player.ViewId, 0);
        }

        if (player.IsLocalHuman)
        {
            localHumanViewId = player.ViewId;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            player.OnNetworkPlayerDied += HandleNetworkPlayerDeath;
        }
    }



    // Rimuove un personaggio morto o disconnesso
    public void UnregisterPlayer(PhotonPlayerController player)
    {
        if (player == null)
        {
            return;
        }

        player.OnNetworkPlayerDied -= HandleNetworkPlayerDeath;

        UnbindPlayerFromHUD(player);

        bool wasRemoved = alivePlayers.Remove(player);

        if (!wasRemoved)
        {
            return;
        }

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom && !isMatchEnded)
        {
            EvaluateMatchState();
        }
    }


    // Riceve sul Master la morte ufficiale di un personaggio, aggiorna le kills 
    // e rivaluta lo stato della partita
    private void HandleNetworkPlayerDeath(PhotonPlayerController deadPlayer, int attackerPlayerViewId)
    {
        if (!PhotonNetwork.IsMasterClient || isMatchEnded || deadPlayer == null)
        {
            return;
        }

        deadPlayer.OnNetworkPlayerDied -= HandleNetworkPlayerDeath;

        alivePlayers.Remove(deadPlayer);

        bool validKill = attackerPlayerViewId > 0 && attackerPlayerViewId != deadPlayer.ViewId;

        if (validKill)
        {
            killCounts.TryGetValue(attackerPlayerViewId, out int currentKillCount);
            killCounts[attackerPlayerViewId] = currentKillCount + 1;
        }

        EvaluateMatchState();
    }



    // Avvia la musica dello scontro finale quando restano due personaggi e 
    // termina il match quando ne resta uno
    private void EvaluateMatchState()
    {
        alivePlayers.RemoveAll(player => player == null);

        if (alivePlayers.Count == 2 && !finalBattleMusicStarted)
        {
            finalBattleMusicStarted = true;

            photonView.RPC(nameof(RPC_PlayFinalBattleMusic), RpcTarget.AllViaServer);
        }

        if (alivePlayers.Count <= 1)
        {
            EndMatch();
        }
    }


    // Costruisce sul Master il risultato ufficiale e lo invia a tutti i Client presenti nell Room
    private void EndMatch()
    {
        if (!PhotonNetwork.IsMasterClient || isMatchEnded)
        {
            return;
        }

        isMatchEnded = true;

        int winnerViewId = alivePlayers.Count == 1 ? alivePlayers[0].ViewId : -1;

        float finalTimeSeconds = uiTimer != null ? uiTimer.GetTime() : 0f;

        int[] playerViewIds = new int[killCounts.Count];

        int[] playerKillCounts = new int[killCounts.Count];

        int index = 0;

        foreach (KeyValuePair<int, int> entry in killCounts)
        {
            playerViewIds[index] = entry.Key;
            playerKillCounts[index] = entry.Value;

            index++;
        }

        photonView.RPC(
            nameof(RPC_EndMatch),
            RpcTarget.AllViaServer,
            winnerViewId,
            finalTimeSeconds,
            playerViewIds,
            playerKillCounts
        );
    }


    // Avvia localmente la musica utilizzata per lo scontro finale.
    [PunRPC]
    private void RPC_PlayFinalBattleMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFinalBattleMusic();
        }
    }


    // Applica su ogni Client il risultato stabilito dal Master 
    // e disabilita definitivamente i personaggi della partita
    [PunRPC]
    private void RPC_EndMatch(int winnerViewId, float finalTimeSeconds, int[] playerViewIds, int[] playerKillCounts)
    {
        isMatchEnded = true;

        string finalTime = GetFormattedFinalTime(finalTimeSeconds);

        PhotonPlayerController[] players = FindObjectsByType<PhotonPlayerController>(FindObjectsSortMode.None);

        foreach (PhotonPlayerController player in players)
        {
            player.SetMatchEnded();

            if (player.IsLocalHuman)
            {
                localHumanViewId = player.ViewId;
            }
        }

        int localKillCount = GetKillCount(localHumanViewId, playerViewIds, playerKillCounts);

        bool localPlayerWon = localHumanViewId > 0 && localHumanViewId == winnerViewId;

        StartCoroutine(ShowEndPanelAfterDelay(localPlayerWon, finalTime, localKillCount));
    }

    // Arresta il timer sul valore ufficiale ricevuto dal Master e restituisce il tempo già formattato
    private string GetFormattedFinalTime(float finalTimeSeconds)
    {
        if (uiTimer != null)
        {
            uiTimer.StopTimerAt(finalTimeSeconds);

            return uiTimer.GetFormattedTime();
        }

        int minutes = Mathf.FloorToInt(finalTimeSeconds / 60f);

        int seconds = Mathf.FloorToInt(finalTimeSeconds % 60f);

        return $"{minutes:00}:{seconds:00}";
    }


    // Cerca il numero di uccisioni associato a uno specifico ViewID.
    private int GetKillCount(int playerViewId, int[] playerViewIds, int[] playerKillCounts)
    {
        if (playerViewIds == null || playerKillCounts == null)
        {
            return 0;
        }

        int count = Mathf.Min(playerViewIds.Length, playerKillCounts.Length);

        for (int index = 0; index < count; index++)
        {
            if (playerViewIds[index] == playerViewId)
            {
                return playerKillCounts[index];
            }
        }

        return 0;
    }



    // Mostra il risultato dopo il ritardo configurato e attiva la gestione Ready della rivincita
    private IEnumerator ShowEndPanelAfterDelay(bool isWin, string finalTime, int localKillCount)
    {
        yield return new WaitForSecondsRealtime(endPanelDelay);

        if (multiplayerResultPanel == null)
        {
            Debug.LogError("[PhotonGameManager] MultiplayerResultPanel non assegnato.", this);
            yield break;
        }

        multiplayerResultPanel.ShowResult(isWin, finalTime, localKillCount.ToString());

        if (photonRematchManager == null)
        {
            Debug.LogError("[PhotonGameManager] PhotonRematchManager non assegnato.", this);
            yield break;
        }

        photonRematchManager.BeginPostMatch();
    }


    // Collega un personaggio al pannello delle vite corrispondeten al suo SlotIndex
    private void BindPlayerToHUD(PhotonPlayerController player)
    {
        if (player == null)
        {
            return;
        }

        if (uiPlayerLives == null || uiPlayerLives.Length == 0)
        {
            Debug.LogWarning("[PhotonGameManager] Array UIPlayerLives non assegnato.", this);
            return;
        }

        int slotIndex = player.SlotIndex;

        if (slotIndex < 0 || slotIndex >= uiPlayerLives.Length)
        {
            Debug.LogWarning("[PhotonGameManager]" + $"SlotIndex HUD non valido: {slotIndex}.", player);
            return;
        }

        UIPlayerLives playerLivesUI = uiPlayerLives[slotIndex];

        if (playerLivesUI == null)
        {
            Debug.LogWarning( "[PhotonGameManager] " + $"UIPlayerLives mancante per lo Slot {slotIndex}.", this);
            return;
        }

        playerLivesUI.Bind(player);
    }



    // Scollega il personaggio dal relativo HUD
    // Quando un plaer abbandona senza morire, il suo indicatore verra indicato come morto
    private void UnbindPlayerFromHUD(PhotonPlayerController player)
    {
        if (player == null || uiPlayerLives == null)
        {
            return;
        }

        int slotIndex = player.SlotIndex;

        if (slotIndex < 0 || slotIndex >= uiPlayerLives.Length)
        {
            return;
        }

        UIPlayerLives playerLivesUI = uiPlayerLives[slotIndex];

        if (playerLivesUI == null || playerLivesUI.GetTargetPhotonPlayer() != player)
        {
            return;
        }

        playerLivesUI.UpdateHearts(0);
        playerLivesUI.Unbind();
    }


    // Avvia la rivincita sul Master
    // Il controllo Ready viene eseguito prima da PhotonRematchManager
    public void RequestReplay()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("[PhotonGameManager] Replay impossibile: il Client non è dentro una Room.", this);
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[PhotonGameManager] Soltanto il Master può avviare la rivincita.", this);
            return;
        }

        if (!isMatchEnded)
        {
            Debug.LogWarning("[PhotonGameManager] Replay richiesto prima della fine della partita.", this);
            return;
        }

        StartReplayAsMaster();
    }


    // Avvia una nuova partita mantenendo la stesso Room e la configurazione corrente dei players
    private void StartReplayAsMaster()
    {
        if (!PhotonNetwork.IsMasterClient || isReplayStarting)
        {
            return;
        }

        isReplayStarting = true;

        StartCoroutine(ReloadMatchRoutine());
    }


    // Rimuove gli oggetti Photon della partita terminata e richiede ad ogni Client di ricaricare la scena
    private IEnumerator ReloadMatchRoutine()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        PhotonNetwork.DestroyAll();
        PhotonNetwork.SendAllOutgoingCommands();

        yield return null;

        photonView.RPC( nameof(RPC_ReloadMatchScene), RpcTarget.AllViaServer, currentSceneName);

        PhotonNetwork.SendAllOutgoingCommands();
    }



    // Ricarica localmente la scena multiplayer mantenendo il Client connesso alla stessa Room Photon
    [PunRPC]
    private void RPC_ReloadMatchScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[PhotonGameManager] Nome della scena di Replay non valido.", this);
            return;
        }

        PhotonNetwork.LoadLevel(sceneName);
    }


    // Rimuove le iscrizioni agli eventi e libera il Singleton.
    private void OnDestroy()
    {
        foreach (PhotonPlayerController player in alivePlayers)
        {
            if (player != null)
            {
                player.OnNetworkPlayerDied -= HandleNetworkPlayerDeath;
            }
        }

        alivePlayers.Clear();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}