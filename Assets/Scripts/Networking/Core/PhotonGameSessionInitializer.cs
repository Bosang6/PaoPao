using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;


/*
 * Ricostruisce localmente la configurazione della partita multiplayer leggendo i dati sincronizzati presenti nella Room Photon.
 *
 * Viene utilizzato soltanto in GameSceneMultiplayer.
 *
 * Responsabilità:
 * - leggere la mappa selezionata dalla Room
 * - leggere gli ActorNumber assegnati ai quattro slot
 * - recuperare il personaggio scelto da ogni player
 * - distinguere LocalHuman, NetworkHuman e AI
 * - riempire gli slot liberi con le AI
 * - configurare GameSession prima degli Start degli altri sistemi
 */

[DefaultExecutionOrder(-1000)]
public sealed class PhotonGameSessionInitializer : MonoBehaviour
{
    [Header("Fallback")]
    [Tooltip("Personaggio utilizzato quando un player non possiede un Character ID valido." )]
    [SerializeField] private CharacterData.E_Character fallbackHumanCharacter = CharacterData.E_Character.Bomberman;



    // Personaggi utilizzati per riempire slot AI
    // con un solo giocatore:
    // Slot 1 -> Slime1
    // Slot 2 -> Slime2
    // Slot 3 -> Slime3
    private static readonly CharacterData.E_Character[] AiCharacters =
    {
        CharacterData.E_Character.Slime1,
        CharacterData.E_Character.Slime2,
        CharacterData.E_Character.Slime3
    };


    private void Awake()
    {
        InitializeMultiplayerSession();
    }


    // Ricostruisce la configurazione completa della partita.
    private void InitializeMultiplayerSession()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogError("[PhotonGameSessionInitializer] " + "Impossibile inizializzare la partita: il client non è dentro una Room Photon.", this);
            return;
        }

        PhotonRoomSlotManager roomSlotManager = ResolveRoomSlotManager();

        if (roomSlotManager == null)
        {
            return;
        }

        E_Map selectedMap = GetSelectedMap();

        List<PlayerSlotConfig> playerSlots = BuildPlayerSlotConfiguration(roomSlotManager);

        if (playerSlots.Count != PhotonRoomSlotManager.SlotCount)
        {
            Debug.LogError("[PhotonGameSessionInitializer] " + $"Configurazione incompleta: trovati " + $"{playerSlots.Count} slot su " + $"{PhotonRoomSlotManager.SlotCount}.", this);
            return;
        }

        GameSession.SetMatchConfig(selectedMap, playerSlots);

        LogConfiguration(selectedMap, playerSlots);
    }


    // Cerca il PhotonRoomSlotManager persistente
    private PhotonRoomSlotManager ResolveRoomSlotManager()
    {
        PhotonConnectionManager connectionManager = PhotonConnectionManager.Instance;

        if (connectionManager == null)
        {
            connectionManager = FindFirstObjectByType<PhotonConnectionManager>();
        }

        if (connectionManager == null)
        {
            Debug.LogError("[PhotonGameSessionInitializer] PhotonConnectionManager non trovato.", this);

            return null;
        }

        PhotonRoomSlotManager roomSlotManager = connectionManager.GetComponent<PhotonRoomSlotManager>();

        if (roomSlotManager == null)
        {
            Debug.LogError("[PhotonGameSessionInitializer] " + "PhotonRoomSlotManager non trovato sul GameObject persistente Networking.", this);

            return null;
        }

        return roomSlotManager;
    }


    // Legge map_id dalle Room Custom Properties
    private E_Map GetSelectedMap()
    {
        if (PhotonRoomProperties.TryGetMapId(PhotonNetwork.CurrentRoom, out int mapId) && Enum.IsDefined(typeof(E_Map), mapId))
        {
            return (E_Map)mapId;
        }

        Debug.LogWarning("[PhotonGameSessionInitializer] " +  "Map ID mancante o non valido. Verrà utilizzata Spring.", this);

        return E_Map.Spring;
    }


    // Costruisce i quattro PlayerSlotConfig.
    private List<PlayerSlotConfig> BuildPlayerSlotConfiguration(PhotonRoomSlotManager roomSlotManager)
    {
        List<PlayerSlotConfig> playerSlots = new List<PlayerSlotConfig>(PhotonRoomSlotManager.SlotCount);

        int nextAiCharacterIndex = 0;

        for (int slotIndex = 0; slotIndex < PhotonRoomSlotManager.SlotCount; slotIndex++)
        {
            int actorNumber = roomSlotManager.GetActorNumberAtSlot(slotIndex);

            if (actorNumber > 0)
            {
                Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

                if (player != null)
                {
                    PlayerSlotConfig humanSlot = CreateHumanSlot(slotIndex, player);

                    playerSlots.Add(humanSlot);
                    continue;
                }

                Debug.LogWarning("[PhotonGameSessionInitializer] " + $"Lo Slot {slotIndex} contiene Actor " + $"{actorNumber}, ma il Player non è presente. " +
                    "Lo slot verrà convertito in AI.", this);
            }

            PlayerSlotConfig aiSlot = CreateAISlot(slotIndex, nextAiCharacterIndex);

            playerSlots.Add(aiSlot);

            nextAiCharacterIndex++;
        }

        return playerSlots;
    }


    // Crea la configurazione di uno slot umano
    private PlayerSlotConfig CreateHumanSlot(int slotIndex, Player player)
    {
        CharacterData.E_Character character = GetPlayerCharacter(player);

        PlayerInstanceData.E_PlayerSlotType playerType = player.IsLocal ? PlayerInstanceData.E_PlayerSlotType.LocalHuman : PlayerInstanceData.E_PlayerSlotType.NetworkHuman;

        return new PlayerSlotConfig(slotIndex, character, playerType);
    }


    // Crea la configurazione di uno slot AI
    private PlayerSlotConfig CreateAISlot(int slotIndex, int aiCharacterIndex)
    {
        CharacterData.E_Character aiCharacter = AiCharacters[aiCharacterIndex % AiCharacters.Length];

        return new PlayerSlotConfig(slotIndex, aiCharacter, PlayerInstanceData.E_PlayerSlotType.AI);
    }


    // Legge character_id dalle proprietà del Player
    private CharacterData.E_Character GetPlayerCharacter(Player player)
    {
        if (PhotonPlayerProperties.TryGetCharacterId(player, out int characterId) && Enum.IsDefined(typeof(CharacterData.E_Character), characterId))
        {
            return (CharacterData.E_Character)characterId;
        }

        Debug.LogWarning("[PhotonGameSessionInitializer] " + $"Character ID non valido per Actor " + $"{player.ActorNumber}. " + $"Verrà utilizzato {fallbackHumanCharacter}.", this);

        return fallbackHumanCharacter;
    }


    // Stampa la configurazione ricostruita localmente
    private void LogConfiguration(E_Map selectedMap, List<PlayerSlotConfig> playerSlots)
    {
        List<string> slotDescriptions = new List<string>();

        foreach (PlayerSlotConfig slot in playerSlots)
        {
            slotDescriptions.Add($"Slot {slot.slotIndex}: " + $"{slot.cType} - {slot.pType}");
        }

        Debug.Log("[PhotonGameSessionInitializer] " + $"Configurazione multiplayer inizializzata. " + $"Mappa: {selectedMap}. " + string.Join(" | ", slotDescriptions));
    }
}