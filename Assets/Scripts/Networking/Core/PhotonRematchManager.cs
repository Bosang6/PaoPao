using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;


/*
 * Gestisce la fase post-partita e la richiesta di rivincita
 *
 * Responsabilità:
 * - sincronizzare il Ready della rivincita
 * - aggiornare le righe del ResultPanel
 * - considerare automaticamente pronte le AI
 * - impedire il Replay quando un giocatore ha lasciato la Room
 * - permettere soltanto al Master di avviare la nuova partita
 */

[DisallowMultipleComponent]
public sealed class PhotonRematchManager : MonoBehaviourPunCallbacks
{
    [Header("Result UI")]
    [SerializeField] private MultiplayerResultPanelController resultPanel;

    [Header("Room Slots")]
    [SerializeField] private PhotonRoomSlotManager roomSlotManager;

    private bool isPostMatchActive;
    private bool localRematchReady;


    private void Awake()
    {
        if (roomSlotManager == null)
        {
            roomSlotManager = FindFirstObjectByType<PhotonRoomSlotManager>();
        }
    }


    private void OnEnable()
    {
        // Registra questo componento come destinatario dei callback Photon
        base.OnEnable();

        if (roomSlotManager != null)
        {
            roomSlotManager.SlotsChanged += HandleSlotsChanged;
        }
    }


    private void Start()
    {
        // Ogni nuova partita parte con il giocatore locale non pronto
        ResetLocalReadyProperty();
    }


    private void OnDisable()
    {
        if (roomSlotManager != null)
        {
            roomSlotManager.SlotsChanged -= HandleSlotsChanged;
        }

        // Rimuove correttamente questo componente dai destinatari dei callback Photon
        base.OnDisable();
    }


    // Viene chiamato dal PhotonGameManager quando compare la schermata del risultato
    public void BeginPostMatch()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("[PhotonRematchManager] Impossibile iniziare la fase post-partita: il client non è dentro una Room.", this);
            return;
        }

        if (resultPanel == null)
        {
            Debug.LogError("[PhotonRematchManager] " + "MultiplayerResultPanel non assegnato.", this);
            return;
        }

        if (roomSlotManager == null)
        {
            Debug.LogError("[PhotonRematchManager] " + "PhotonRoomSlotManager non trovato.", this);
            return;
        }

        isPostMatchActive = true;

        localRematchReady = false;

        PhotonPlayerProperties.SetRematchReady(PhotonNetwork.LocalPlayer, false);

        RefreshAllRows();
        RefreshPlayAgainButton();
    }


    // Evento pulsante PlayAgain, se premuto diventa ready, altrimenti non ready 
    // Quando tutti sono pronti, il Master utilizza lo stesso pulsante per avviare il Replay
    public void OnPlayAgainPressed()
    {
        if (!isPostMatchActive || !PhotonNetwork.InRoom)
        {
            return;
        }

        if (HasMissingHumanPlayer())
        {
            Debug.LogWarning( "[PhotonRematchManager] " + "Replay non disponibile: un giocatore ha lasciato la Room.", this);
            return;
        }


        // Quando tutti sono pronti, il pulsante sul Master diventa START MATCH
        if (PhotonNetwork.IsMasterClient && AreAllHumanPlayersReady())
        {
            if (PhotonGameManager.Instance == null)
            {
                Debug.LogError("[PhotonRematchManager] " + "PhotonGameManager non presente nella scena.", this);
                return;
            }

            resultPanel.SetPlayAgainButtonState( "STARTING...", false);

            PhotonGameManager.Instance.RequestReplay();
            return;
        }

        bool newReadyState = !localRematchReady;

        bool requestAccepted = PhotonPlayerProperties.SetRematchReady(PhotonNetwork.LocalPlayer, newReadyState);

        if (!requestAccepted)
        {
            Debug.LogError("[PhotonRematchManager] Photon non ha accettato il cambio di stato Ready.", this);
            return;
        }


        // Aggiornamento immediato locale, il Callback Photon confermerà poi il valore anche su tutti i client
        localRematchReady = newReadyState;

        RefreshAllRows();
        RefreshPlayAgainButton();
    }



    // Photon richiama questo callback su tutti i client quando cambia una Player Custom Property
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProperties)
    {
        if (!isPostMatchActive || targetPlayer == null || changedProperties == null)
        {
            return;
        }

        if (!changedProperties.ContainsKey(PhotonPlayerProperties.RematchReadyKey))
        {
            return;
        }

        if (targetPlayer.IsLocal && PhotonPlayerProperties.TryGetRematchReady(targetPlayer, out bool ready))
        {
            localRematchReady = ready;
        }

        RefreshAllRows();
        RefreshPlayAgainButton();
    }


    // Aggiorna la tabella se un player lascia la Room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!isPostMatchActive)
        {
            return;
        }

        RefreshAllRows();
        RefreshPlayAgainButton();
    }


    // Aggiorna il pulsante quando cambia il Master
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (!isPostMatchActive)
        {
            return;
        }

        RefreshAllRows();
        RefreshPlayAgainButton();
    }


    private void HandleSlotsChanged()
    {
        if (!isPostMatchActive)
        {
            return;
        }

        RefreshAllRows();
        RefreshPlayAgainButton();
    }


    // Aggionra tutte le righe 
    private void RefreshAllRows()
    {
        if (resultPanel == null || roomSlotManager == null || GameSession.PlayerSlots == null)
        {
            return;
        }

        for (int slotIndex = 0; slotIndex < PhotonRoomSlotManager.SlotCount; slotIndex++)
        {
            PlayerSlotConfig slot = GetSlotConfiguration(slotIndex);

            if (slot == null)
            {
                resultPanel.SetPlayerStatus(slotIndex, E_RematchPlayerStatus.Left);
                continue;
            }

            if (slot.pType == PlayerInstanceData.E_PlayerSlotType.AI)
            {
                resultPanel.SetPlayerStatus(slotIndex, E_RematchPlayerStatus.AI);
                continue;
            }

            int actorNumber = roomSlotManager.GetActorNumberAtSlot(slotIndex);

            if (actorNumber <= 0)
            {
                resultPanel.SetPlayerStatus(slotIndex, E_RematchPlayerStatus.Left);
                continue;
            }

            Player photonPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

            if (photonPlayer == null)
            {
                resultPanel.SetPlayerStatus(slotIndex, E_RematchPlayerStatus.Left);
                continue;
            }

            bool isReady = GetPlayerRematchReady(photonPlayer);

            resultPanel.SetPlayerStatus(slotIndex, isReady ? E_RematchPlayerStatus.Ready : E_RematchPlayerStatus.NotReady);
        }
    }



    // Aggiona testo e comportamento del pulsante locale
    private void RefreshPlayAgainButton()
    {
        if (resultPanel == null)
        {
            return;
        }

        if (HasMissingHumanPlayer())
        {
            resultPanel.SetPlayAgainButtonState("PLAYER LEFT", false);
            return;
        }

        if (PhotonNetwork.IsMasterClient && AreAllHumanPlayersReady())
        {
            resultPanel.SetPlayAgainButtonState("START MATCH", true);
            return;
        }

        resultPanel.SetPlayAgainButtonState(localRematchReady ? "CANCEL READY" : "PLAY AGAIN", true);
    }


    // Controlla che tutti gli umani ancora previsti dalla partita siano presenti e Ready
    private bool AreAllHumanPlayersReady()
    {
        if (GameSession.PlayerSlots == null || roomSlotManager == null)
        {
            return false;
        }

        bool foundHumanPlayer = false;

        foreach (PlayerSlotConfig slot in GameSession.PlayerSlots)
        {
            if (slot == null || slot.pType == PlayerInstanceData.E_PlayerSlotType.AI)
            {
                continue;
            }

            foundHumanPlayer = true;
            
            int actorNumber = roomSlotManager.GetActorNumberAtSlot(slot.slotIndex);

            if (actorNumber <= 0)
            {
                return false;
            }

            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

            if (player == null || !GetPlayerRematchReady(player))
            {
                return false;
            }
        }

        return foundHumanPlayer;
    }


    /*
     * Restituisce true quando uno slot originariamente umano
     * non è più occupato da un Photon Player.
     *
     * In questo caso il Replay viene bloccato e il Master
     * dovrà usare RETURN MENU.
     */
    // Restituisce true quando uno slot umano non è più occupato da un Photon Player
    // In questo caso il Replay viene bloccato ed il Master dovrà usare Return Menu
    private bool HasMissingHumanPlayer()
    {
        if (GameSession.PlayerSlots == null || roomSlotManager == null)
        {
            return true;
        }

        foreach (PlayerSlotConfig slot in GameSession.PlayerSlots)
        {
            if (slot == null || slot.pType == PlayerInstanceData.E_PlayerSlotType.AI)        
            {
                continue;
            }

            int actorNumber = roomSlotManager.GetActorNumberAtSlot(slot.slotIndex);

            if (actorNumber <= 0)
            {
                return true;
            }

            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

            if (player == null)
            {
                return true;
            }
        }

        return false;
    }


    private bool GetPlayerRematchReady(Player player)
    {
        if (player == null)
        {
            return false;
        }

        // Il valore locale viene aggionato immediatamente senza aspettare il round di Photon
        if (player.IsLocal)
        {
            return localRematchReady;
        }

        return PhotonPlayerProperties.TryGetRematchReady(player, out bool ready) && ready;
    }


    private PlayerSlotConfig GetSlotConfiguration(int slotIndex)
    {
        if (GameSession.PlayerSlots == null)
        {
            return null;
        }

        foreach (PlayerSlotConfig slot in GameSession.PlayerSlots)
        {
            if (slot != null && slot.slotIndex == slotIndex)
            {
                return slot;
            }
        }

        return null;
    }


    private void ResetLocalReadyProperty()
    {
        localRematchReady = false;

        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
        {
            return;
        }

        PhotonPlayerProperties.SetRematchReady(PhotonNetwork.LocalPlayer, false);
    }
}