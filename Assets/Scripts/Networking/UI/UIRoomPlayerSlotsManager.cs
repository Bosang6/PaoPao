using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/*
 * Collega i 4 slot grafici del RoomLobbyPanel agli slot sincronizzati dal PhotonRoomSlotManager
 * 
 * 
 * Responsabilità:
 * - leggere l'ActorNumber assegnato a ogni slot
 * - capire se lo slot è occupato oppure libero
 * - individuare il giocatore locale
 * - individuare il Master Client
 * - aggiornare la UI quando le assegnazioni cambiano
 * 
 */

public sealed class UIRoomPlayerSlotsManager : MonoBehaviourPunCallbacks
{
    [Header("Player Slots")]
    [Tooltip("I quattro PlayerSlot ordinati dallo Slot 0 allo Slot 3.")]
    [SerializeField] private RoomPlayerSlotUI[] playerSlots;

    [Header("Character Sprites")]
    [Tooltip("Sprite ordinati secondo i Character ID: " + "Bomberman, Penguin, Slime1, Slime2, Slime3.")]
    [SerializeField] private Sprite[] characterSprites;

    [Tooltip("Personaggio mostrato quando un player non possiede " + "ancora una proprietà Character ID valida." )]
    [SerializeField] [Min(0)] private int fallbackCharacterId = 0;

    private PhotonRoomSlotManager roomSlotManager;

    // Viene eseguito quando la PlayerSection o il RoomLobbyPanel vengono attivati
    public override void OnEnable()
    {
        base.OnEnable();

        if (!ValidateCharacterSprites())
        {
            ClearSlots();
            return;
        }

        if (!ResolveRoomSlotManager())
        {
            ClearSlots();
            return;
        }

        roomSlotManager.SlotsChanged += HandleSlotsChanged;
        RefreshSlots();
    }

    // Quando vengono disabilitati rimuove il listener
    public override void OnDisable()
    {
        if (roomSlotManager != null)
        {
            roomSlotManager.SlotsChanged -= HandleSlotsChanged;
        }

        base.OnDisable();
    }


    // Cerca il PhotonRoomSlotManager sul GameObject Networking  
    private bool ResolveRoomSlotManager()
    {
        PhotonConnectionManager connectionManager = PhotonConnectionManager.Instance;

        if (connectionManager == null)
        {
            connectionManager = FindFirstObjectByType<PhotonConnectionManager>();
        }

        if (connectionManager == null)
        {
            Debug.LogError("[UIRoomPlayerSlotsManager] " + "PhotonConnectionManager non trovato.", this);

            return false;
        }

        roomSlotManager = connectionManager.GetComponent<PhotonRoomSlotManager>();

        if (roomSlotManager == null)
        {
            Debug.LogError("[UIRoomPlayerSlotsManager] " + "PhotonRoomSlotManager non trovato " + "sul GameObject Networking.", this);

            return false;
        }

        return true;
    }


    // Viene richiamato dal PhotonRoomSlotManager quando le assegnazioni degli slot cambiano
    private void HandleSlotsChanged()
    {
        RefreshSlots();
    }


    // Aggiorna tutti e 4 gli elementi grafici 
    private void RefreshSlots()
    {
        if (playerSlots == null || playerSlots.Length != PhotonRoomSlotManager.SlotCount)
        {
            Debug.LogError("[UIRoomPlayerSlotsManager] Devono essere " + "assegnati esattamente quattro PlayerSlot.", this);

            return;
        }

        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null || roomSlotManager == null)
        {
            ClearSlots();
            return;
        }

        for (int slotIndex = 0; slotIndex < playerSlots.Length; slotIndex++)
        {
            RoomPlayerSlotUI slotUI = playerSlots[slotIndex];

            if (slotUI == null)
            {
                Debug.LogWarning( $"[UIRoomPlayerSlotsManager] " + $"PlayerSlot {slotIndex} non assegnato.", this);

                continue;
            }

            int actorNumber = roomSlotManager.GetActorNumberAtSlot(slotIndex);

            // ActorNumber 0 significa slot libero.
            if (actorNumber <= 0)
            {
                slotUI.ShowEmpty();
                continue;
            }

            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

            // La proprietà potrebbe essere arrivata prima
            // della lista aggiornata dei Player.
            if (player == null)
            {
                slotUI.ShowEmpty();
                continue;
            }

            int characterId = GetCharacterId(player);

            Sprite characterSprite = characterSprites[characterId];

            slotUI.ShowOccupied(player.IsLocal, player.IsMasterClient, characterSprite);
        }
    }



    // Restituisce il Character ID valido associato al Player Photon
    private int GetCharacterId(Player player)
    {
        if (PhotonPlayerProperties.TryGetCharacterId(player, out int characterId) && IsValidCharacterId(characterId)
        )
        {
            return characterId;
        }

        return fallbackCharacterId;
    }


    // Verifica che il Character ID corrisponde ad uno sprite configurato nell'Inspector
    private bool IsValidCharacterId(int characterId)
    {
        return
            characterSprites != null &&
            characterId >= 0 &&
            characterId < characterSprites.Length &&
            characterSprites[characterId] != null;
    }


    // Ripristina tutti gli slot allo stato libero
    private void ClearSlots()
    {
        if (playerSlots == null)
        {
            return;
        }

        foreach (RoomPlayerSlotUI playerSlot in playerSlots)
        {
            if (playerSlot != null)
            {
                playerSlot.ShowEmpty();
            }
        }
    }

    // Aggiorna gli slot quando il pannello è già attivo ed il client entra in una Room
    public override void OnJoinedRoom()
    {
        RefreshSlots();
    }

    // Quando il MasterClient cambia, aggiorna le corone
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        RefreshSlots();
    }


    // Pulisce la UI dopo che il client locale ha lasciato la Room
    public override void OnLeftRoom()
    {
        ClearSlots();
    }


    // Viene chiamato quando cambiano le Player Custom Properties di un giocatore
    // Se è cambiato il Char ID, aggiorna tutti gli slot
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProperties)
    {
        if (targetPlayer == null || changedProperties == null)
        {
            return;
        }

        if (!changedProperties.ContainsKey(PhotonPlayerProperties.CharacterIdKey))
        {
            return;
        }

        RefreshSlots();
    }


    // Controlla che gli sprite dei personaggi siano stati configurati correttamente
    private bool ValidateCharacterSprites()
    {
        if (characterSprites == null || characterSprites.Length == 0)
        {
            Debug.LogError("[UIRoomPlayerSlotsManager] " + "Nessuno sprite dei personaggi assegnato.", this);

            return false;
        }

        if (!IsValidCharacterId(fallbackCharacterId))
        {
            Debug.LogError("[UIRoomPlayerSlotsManager] " + $"Fallback Character ID non valido: " + $"{fallbackCharacterId}.", this);

            return false;
        }

        for (int characterId = 0; characterId < characterSprites.Length; characterId++)
        {
            if (characterSprites[characterId] != null)
            {
                continue;
            }

            Debug.LogError("[UIRoomPlayerSlotsManager] " + $"Sprite mancante per Character ID " + $"{characterId}.",this);

            return false;
        }

        return true;
    }


}
