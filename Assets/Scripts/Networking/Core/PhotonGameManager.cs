using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;


/*
 * Gestisce lo stato autoritativo della partita multiplayer
 * 
 * Responsabilità:
 * - registrare tutti i PhotonPlayerController
 * - ricevere gli eventi di morte sul Master Client
 * - mantenere il numero dei personaggi ancora vivi
 * - attribuire le uccisioni
 * - avviare la musica dello scontro finale
 * - stabilire il vincitore
 * - mostrare Win/Lose su ogni client
 */


[DisallowMultipleComponent]
[RequireComponent(typeof(PhotonView))]
public sealed class PhotonGameManager : MonoBehaviourPun
{
    public static PhotonGameManager Instance
    {
        get;
        private set;
    }

    [Header("UI")]
    [SerializeField] private UIGameManager uiGameManager;

    [Header("Player HUD")]
    [SerializeField] private UIPlayerLives[] uiPlayerLives;

    [Header("Timer")]
    [SerializeField] private UITimer uiTimer;

    [Header("End Game")]
    [SerializeField] [Min(0f)] private float endPanelDelay = 1f;

    [Header("Multiplayer Result Panel")]
    [SerializeField] private MultiplayerResultPanelController multiplayerResultPanel;

    private readonly List<PhotonPlayerController> alivePlayers = new List<PhotonPlayerController>();

    private readonly Dictionary<int, int> killCounts = new Dictionary<int, int>();

    private int localHumanViewId = -1;

    private bool finalBattleMusicStarted;

    private bool isMatchEnded;

    // Impedisce di avviare più ricaricamenti contemporaneamente.
    private bool isReplayStarting;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }


    private void Start()
    {
        Time.timeScale = 1f;

        // Normalmente i personaggi si registrano da soli.
        // Questa ricerca copre anche il caso in cui un player sia stato inizializzato prima dello Start del Manager
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


    // Regista un personaggio su ogni client. Soltanto il Maste si iscrive all'evento authoritative di morte
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

        Debug.Log("[PhotonGameManager] " + $"Registrato Player ViewID {player.ViewId}. " + $"Giocatori vivi: {alivePlayers.Count}.");
    }


    // 
    public void UnregisterPlayer(PhotonPlayerController player)
    {
        if (player == null)
        {
            return;
        }

        player.OnNetworkPlayerDied -= HandleNetworkPlayerDeath;

        UnbindPlayerFromHUD(player);
        bool wasRemoved = alivePlayers.Remove(player);

        // Se il player era già stato rimosso dalla morte, non valutiamo nuovamente il risultato
        if (!wasRemoved)
        {
            return;
        }

        if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom && !isMatchEnded)
        {
            EvaluateMatchState();
        }
    }


    // Riceve sul Master la morte ufficiale del personaggio 
    private void HandleNetworkPlayerDeath(PhotonPlayerController deadPlayer, int attackerPlayerViewId)
    {
        if (!PhotonNetwork.IsMasterClient || isMatchEnded || deadPlayer == null)
        {
            return;
        }

        deadPlayer.OnNetworkPlayerDied -= HandleNetworkPlayerDeath;

        alivePlayers.Remove(deadPlayer);

        // Non assgniamo una kill qunado un giocatore viene eliminato dalla propria bomba
        if (attackerPlayerViewId > 0 && attackerPlayerViewId != deadPlayer.ViewId)
        {
            if (!killCounts.ContainsKey(attackerPlayerViewId))
            {
                killCounts.Add(attackerPlayerViewId,0);
            }

            killCounts[attackerPlayerViewId]++;
        }

        Debug.Log("[PhotonGameManager] " + $"Player ViewID {deadPlayer.ViewId} eliminato. " + $"Attaccante ViewID: {attackerPlayerViewId}. " + $"Rimasti: {alivePlayers.Count}.");

        EvaluateMatchState();
    }


    // Controlla musica finale e condizione di vittoria
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


    // Il Master costruisce il risultato ufficiale e lo invia a tutti i client
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


    [PunRPC]
    private void RPC_PlayFinalBattleMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFinalBattleMusic();
        }
    }


    // Applica localmente il risultato deciso dal Master
    [PunRPC]
    private void RPC_EndMatch(int winnerViewId, float finalTimeSeconds, int[] playerViewIds, int[] playerKillCounts)
    {
        isMatchEnded = true;

        string finalTime = "00:00";

        if (uiTimer != null)
        {
            uiTimer.StopTimerAt(finalTimeSeconds);
            finalTime = uiTimer.GetFormattedTime();
        }
        else
        {
            int minutes = Mathf.FloorToInt(finalTimeSeconds / 60f);
            int seconds = Mathf.FloorToInt(finalTimeSeconds % 60f);
            finalTime = $"{minutes:00}:{seconds:00}";
        }

PhotonPlayerController[] players = FindObjectsByType<PhotonPlayerController>(FindObjectsSortMode.None);

        foreach (PhotonPlayerController player in players)
        {
            player.SetMatchEnded();

            if (player.IsLocalHuman)
            {
                localHumanViewId = player.ViewId;
            }
        }

        int localKillCount =  GetKillCount(localHumanViewId, playerViewIds, playerKillCounts);

        bool localPlayerWon = localHumanViewId > 0 && localHumanViewId == winnerViewId;

        StartCoroutine(ShowEndPanelAfterDelay(localPlayerWon, finalTime, localKillCount.ToString()));
    }

    //
    private int GetKillCount(int playerViewId, int[] playerViewIds, int[] playerKillCounts)
    {
        if (playerViewIds == null || playerKillCounts == null)
        {
            return 0;
        }

        int count = Mathf.Min(playerViewIds.Length, playerKillCounts.Length);

        for (int i = 0; i < count; i++)
        {
            if (playerViewIds[i] == playerViewId)
            {
                return playerKillCounts[i];
            }
        }

        return 0;
    }

    //
    private IEnumerator ShowEndPanelAfterDelay(bool isWin, string finalTime, string localKillCounter)
    {
        yield return new WaitForSecondsRealtime(endPanelDelay);

        if (multiplayerResultPanel == null)
        {
            Debug.LogError("[PhotonGameManager] " + "MultiplayerResultPanel non assegnato.", this);
            yield break;
        }

        multiplayerResultPanel.ShowResult(isWin, finalTime, localKillCounter);
    }

   

    //
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


    // Collega un PhotonPlayerController al pannello delle vita corrispondente al suo SlotIndex
    // Questo metodo viene eseguito su ogni client, compresi i personaggi ricevuti automaticamente dalla rete
    private void BindPlayerToHUD(PhotonPlayerController player)
    {
        if (player == null) return;

        if (uiPlayerLives == null || uiPlayerLives.Length == 0)
        {
            Debug.LogWarning("[PhotonGameManager] Array UIPlayerLives non assegnato.", this);
            return;
        }

        int slotIndex = player.SlotIndex;

        if (slotIndex < 0 || slotIndex >= uiPlayerLives.Length)
        {
            Debug.LogWarning("[PhotonGameManager] " + $"SlotIndex HUD non valido: {slotIndex}.", player);
            return;
        }

        UIPlayerLives playerLivesUI = uiPlayerLives[slotIndex];

        if (playerLivesUI == null)
        {
            Debug.LogWarning("[PhotonGameManager] " + $"UIPlayerLives mancante per lo Slot {slotIndex}.", this);
            return;
        }

        playerLivesUI.Bind(player);

        Debug.Log("[PhotonGameManager] " + $"HUD vite collegato allo Slot {slotIndex}, " + $"ViewID {player.ViewId}.");
    }


    // Scollega il personaggio dal relativo HUD
    // Se il personaggio lascia la partita senza morire, il suo pannello viene comunque mostrato come eliminato
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

        if (playerLivesUI == null)
        {
            return;
        }

        if (playerLivesUI.GetTargetPhotonPlayer() != player)
        {
            return;
        }

        playerLivesUI.UpdateHearts(0);

        playerLivesUI.Unbind();
    }


    // Riceva la richiesta di Replay dalla UI locale
    // Il Master può avviare direttamente il caricamento
    // Un client invece invia la richiesta al Master.
    public void RequestReplay()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("[PhotonGameManager] Replay impossibile: il client non è dentro una Room.", this);
            return;
        }

        if (!isMatchEnded)
        {
            Debug.LogWarning("[PhotonGameManager] Replay richiesto prima della fine della partita.", this);
            return;
        }

        if (isReplayStarting)
        {
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            StartReplayAsMaster();
            return;
        }

        photonView.RPC(
            nameof(RPC_RequestReplay),
            RpcTarget.MasterClient
        );
    }


    // Riceve sul Master la richiesta inviata da un altro giocatore
    [PunRPC]
    private void RPC_RequestReplay(PhotonMessageInfo messageInfo)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (!isMatchEnded || isReplayStarting)
        {
            return;
        }

        Debug.Log("[PhotonGameManager] " + $"Replay richiesto dall'Actor {messageInfo.Sender.ActorNumber}.");

        StartReplayAsMaster();
    }


    // Avvia una nuova partita mantenendo la stessa Room, gli stessi player e la stessa configurazione
    private void StartReplayAsMaster()
    {
        if (!PhotonNetwork.IsMasterClient || isReplayStarting)
        {
            return;
        }

        isReplayStarting = true;
        StartCoroutine(ReloadMatchRoutine());
    }


    // Rimuove gli oggetti Photon dalla partita terminata
    private IEnumerator ReloadMatchRoutine()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Elimina player, bombe, e altri oggetti creati tramite Photon
        // Evita che gli eventi di istanziazione della vecchia partita rimangono memorizzati nella Room
        PhotonNetwork.DestroyAll();

        PhotonNetwork.SendAllOutgoingCommands();

        yield return null;

        // Il cambio scena viene replicato automaticamente a tutti i Client
        PhotonNetwork.LoadLevel(currentSceneName);
    }




}
