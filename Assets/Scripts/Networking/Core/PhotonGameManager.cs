using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;


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

    [Header("Timer")]
    [SerializeField] private UITimer uiTimer;

    [Header("End Game")]
    [SerializeField] [Min(0f)] private float endPanelDelay = 1f;


    private readonly List<PhotonPlayerController> alivePlayers = new List<PhotonPlayerController>();

    private readonly Dictionary<int, int> killCounts = new Dictionary<int, int>();

    private int localHumanViewId = -1;

    private bool finalBattleMusicStarted;

    private bool isMatchEnded;

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



    public void UnregisterPlayer(PhotonPlayerController player)
    {
        if (player == null)
        {
            return;
        }

        player.OnNetworkPlayerDied -= HandleNetworkPlayerDeath;

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

        string finalTime = uiTimer != null ? uiTimer.GetFormattedTime() : "00:00";

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
            finalTime,
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
    private void RPC_EndMatch(int winnerViewId, string finalTime, int[] playerViewIds, int[] playerKillCounts)
    {
        isMatchEnded = true;

        if (uiTimer != null)
        {
            uiTimer.StopTimer();
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


    private IEnumerator ShowEndPanelAfterDelay(bool isWin, string finalTime, string localKillCounter)
    {
        yield return new WaitForSecondsRealtime(endPanelDelay);

        if (uiGameManager == null)
        {
            yield break;
        }

        if (isWin)
        {
            uiGameManager.ShowMultiplayerWinPanel(finalTime, localKillCounter);
        }
        else
        {
            uiGameManager.ShowMultiplayerLosePanel(finalTime, localKillCounter);
        }
    }


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
