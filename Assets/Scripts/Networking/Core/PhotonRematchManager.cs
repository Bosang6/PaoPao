using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;


/*
 * Gestisce il sistema di rivincita al termine della partita.
 *
 * Sincronizza il Ready dei giocatori, aggiorna la tabella
 * dei risultati e permette al Master di avviare il Replay.
 */ 

[DisallowMultipleComponent]
public sealed class PhotonRematchManager : MonoBehaviourPunCallbacks
{
    private const string PlayAgainLabel = "PLAY AGAIN";
    private const string CancelReadyLabel = "CANCEL READY";
    private const string StartMatchLabel = "START MATCH";
    private const string StartingLabel = "STARTING...";
    private const string PlayerLeftLabel = "PLAYER LEFT";

    [Header("Result UI")]
    [SerializeField] private MultiplayerResultPanelController resultPanel;

    [Header("Room Slots")]
    [SerializeField] private PhotonRoomSlotManager roomSlotManager;

    private bool isPostMatchActive;
    private bool localRematchReady;


    // Recupera il gestore degli slot quando non è assegnato nell'Inspector.
    private void Awake()
    {
        if (roomSlotManager == null)
        {
            roomSlotManager = FindFirstObjectByType<PhotonRoomSlotManager>();
        }
    }


    // Registra i callback Photon e gli aggiornamenti degli slot.
    private void OnEnable()
    {
        base.OnEnable();

        if (roomSlotManager != null)
        {
            roomSlotManager.SlotsChanged += HandleSlotsChanged;
        }
    }


    // Azzera il Ready della rivincita all'inizio di ogni partita.
    private void Start()
    {
        ResetLocalReadyProperty();
    }


    // Rimuove le iscrizioni prima di disattivare il componente.
    private void OnDisable()
    {
        if (roomSlotManager != null)
        {
            roomSlotManager.SlotsChanged -= HandleSlotsChanged;
        }

        base.OnDisable();
    }


    // Attiva la fase post partita ed inizializza il giocatore locale come non pronto per la rivincita
    public void BeginPostMatch()
    {
        if (!ValidatePostMatchState())
        {
            return;
        }

        isPostMatchActive = true;
        localRematchReady = false;

        bool requestStarted = PhotonPlayerProperties.SetRematchReady(PhotonNetwork.LocalPlayer, false);

        if (!requestStarted)
        {
            Debug.LogWarning("[PhotonRematchManager] Impossibile inizializzare il Ready locale.", this);
        }

        RefreshPostMatchUI();
    }


    // Cambia il Ready del giocatore locale
    // Quando tutti i player sono pronti il Master utilizza lo stesso pulsante per avviare la nuova partita
    public void OnPlayAgainPressed()
    {
        if (!isPostMatchActive || !PhotonNetwork.InRoom)
        {
            return;
        }

        if (HasMissingHumanPlayer())
        {
            Debug.LogWarning("[PhotonRematchManager] Replay non disponibile: un giocatore ha lasciato la Room.", this);
            return;
        }

        if (PhotonNetwork.IsMasterClient && AreAllHumanPlayersReady())
        {
            StartReplay();
            return;
        }

        ToggleLocalReady();
    }



    // Aggiorna lo stato qunado cambia una Custom Property relativa al Ready della rivincita
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

        if (targetPlayer.IsLocal && PhotonPlayerProperties.TryGetRematchReady(targetPlayer, out bool isReady))
        {
            localRematchReady = isReady;
        }

        RefreshPostMatchUI();
    }


    // Aggiorna la tabella quando un giocatore lascia la Room.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!isPostMatchActive)
        {
            return;
        }

        RefreshPostMatchUI();
    }


    // Aggiorna la schermata quando cambia l'assegnazione degli slot.
    private void HandleSlotsChanged()
    {
        if (!isPostMatchActive)
        {
            return;
        }

        RefreshPostMatchUI();
    }


    // Controlla che la fase post-partita possa essere avviata.
    private bool ValidatePostMatchState()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("[PhotonRematchManager] Il Client non si trova dentro una Room.", this);
            return false;
        }

        if (resultPanel == null)
        {
            Debug.LogError("[PhotonRematchManager] MultiplayerResultPanel non assegnato.", this);
            return false;
        }

        if (roomSlotManager == null)
        {
            Debug.LogError("[PhotonRematchManager] PhotonRoomSlotManager non trovato.", this);
            return false;
        }

        return true;
    }


    // Inverte e sincronizza il Ready del giocatore locale.
    private void ToggleLocalReady()
    {
        bool newReadyState = !localRematchReady;

        bool requestStarted = PhotonPlayerProperties.SetRematchReady(PhotonNetwork.LocalPlayer, newReadyState);

        if (!requestStarted)
        {
            Debug.LogError("[PhotonRematchManager] Photon non ha accettato il cambio di Ready.", this);
            return;
        }


        // Aggiorniamo subito il valore locale senza attendere la conferma della Custom Prop da photon
        localRematchReady = newReadyState;

        RefreshPostMatchUI();
    }


    // Richiede al PhotonGameManager di avviare la rivincita.
    private void StartReplay()
    {
        if (PhotonGameManager.Instance == null)
        {
            Debug.LogError("[PhotonRematchManager] PhotonGameManager non presente nella scena.", this);
            return;
        }

        resultPanel.SetPlayAgainButtonState(StartingLabel, false);

        PhotonGameManager.Instance.RequestReplay();
    }


    // Aggiorna la tabella e il pulsante della rivincita.
    private void RefreshPostMatchUI()
    {
        RefreshAllRows();
        RefreshPlayAgainButton();
    }


    // Aggioran ogni riga in base al tipo di slot, alla presenza ed allo stato del Ready del Player
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
                SetPlayerStatus(slotIndex, E_RematchPlayerStatus.Left);
                continue;
            }

            if (slot.pType == PlayerInstanceData.E_PlayerSlotType.AI)
            {
                SetPlayerStatus(slotIndex, E_RematchPlayerStatus.AI);
                continue;
            }

            Player photonPlayer = GetPhotonPlayerAtSlot(slotIndex);

            if (photonPlayer == null)
            {
                SetPlayerStatus(slotIndex, E_RematchPlayerStatus.Left);
                continue;
            }

            bool isReady = GetPlayerRematchReady(photonPlayer);

            SetPlayerStatus(slotIndex, isReady ? E_RematchPlayerStatus.Ready : E_RematchPlayerStatus.NotReady);
        }
    }


    // Imposta lo stato grafico di una riga del pannello.
    private void SetPlayerStatus(int slotIndex, E_RematchPlayerStatus status)
    {
        resultPanel.SetPlayerStatus(slotIndex, status);
    }


    // Aggiorna testo e interazione del pulsante locale in base allo stato corrente dell rivincita
    private void RefreshPlayAgainButton()
    {
        if (resultPanel == null)
        {
            return;
        }

        if (HasMissingHumanPlayer())
        {
            resultPanel.SetPlayAgainButtonState(PlayerLeftLabel, false);
            return;
        }

        if (PhotonNetwork.IsMasterClient && AreAllHumanPlayersReady())
        {
            resultPanel.SetPlayAgainButtonState(StartMatchLabel, true);
            return;
        }

        resultPanel.SetPlayAgainButtonState(localRematchReady ? CancelReadyLabel : PlayAgainLabel, true);
    }



    // Restituisce true quando tutti i player umani ancora presenti hanno confermato il Ready
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

            Player player = GetPhotonPlayerAtSlot(slot.slotIndex);

            if (player == null || !GetPlayerRematchReady(player))
            {
                return false;
            }
        }

        return foundHumanPlayer;
    }



    // Restituisce true quando uno slot umano non è più occupato da un player Photon
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

            if (GetPhotonPlayerAtSlot(slot.slotIndex) == null)
            {
                return true;
            }
        }

        return false;
    }


    // Restituisce il giocatore Photon assegnato a uno slot.
    private Player GetPhotonPlayerAtSlot(int slotIndex)
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null || roomSlotManager == null)
        {
            return null;
        }

        int actorNumber = roomSlotManager.GetActorNumberAtSlot(slotIndex);

        if (actorNumber <= 0)
        {
            return null;
        }

        return PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
    }



    // Legge il Ready di un player.
    private bool GetPlayerRematchReady(Player player)
    {
        if (player == null)
        {
            return false;
        }

        if (player.IsLocal)
        {
            return localRematchReady;
        }

        return PhotonPlayerProperties.TryGetRematchReady(player, out bool isReady) && isReady;
    }


    // Cerca la configurazione corrispondente a uno SlotIndex.
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


    // Reimposta la Custom Property locale all'inizio del match.
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