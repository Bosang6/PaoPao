using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;


/*
 * Gestisce l'assegnazione dei 4 slot umani all'interno di una Room Photon
 * 
 * Gli slots sono memorizzati nelle Room Custom Properties:
 * slot_0 -> ActorNumber del player
 * slot_1 -> ActorNumber del player
 * slot_2 -> ActorNumber del player
 * slot_3 -> ActorNumber del player
 * 
 * Il valore 0 indica uno slot libero.
 * 
 * 
 */


public sealed class PhotonRoomSlotManager : MonoBehaviourPunCallbacks
{
    public const int SlotCount = 4;

    private const string SlotKeyPrefix = "slot_";

    // Evento notificato quando la configurazione degli slot cambia 
    public event Action SlotsChanged;


    // Costruisce le proprietà iniziali della Room con tutti gli slot liberi (valore 0)
    public static Hashtable CreateInitialRoomProperties()
    {
        Hashtable properties = new Hashtable();

        for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
        {
            properties[GetSlotKey(slotIndex)] = 0;
        }

        return properties;
    }


    // Restituisce l'ActorNumber associato allo slot
    //
    // Restituisce 0 se:
    // - non siamo in una Room
    // - l'indice non è valido
    // - lo slot è libero
    // - la proprietà non esiste
    public int GetActorNumberAtSlot(int slotIndex)
    {
        // Non siamo in una Room
        if (!PhotonNetwork.InRoom)
        {
            return 0;
        }

        // Indice non valido
        if (!IsValidSlotIndex(slotIndex))
        {
            Debug.LogWarning($"[PhotonRoomSlotManager] Slot non valido: {slotIndex}.");

            return 0;
        }

        string slotKey = GetSlotKey(slotIndex);

        // Proprietà non esistente
        if (!PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(slotKey, out object storedValue))
        {
            return 0;
        }

        // Valore non valido
        if (storedValue == null)
        {
            return 0;
        }

        try
        {
            return Convert.ToInt32(storedValue);
        }
        catch (Exception exception)
        {
            Debug.LogError($"[PhotonRoomSlotManager] Valore non valido per " + $"{slotKey}: {storedValue}. " + $"Errore: {exception.Message}");

            return 0;
        }
    }


    // Cerca lo slot occupato da uno specifico player
    public bool TryGetSlotForPlayer(Player player, out int slotIndex)
    {
        slotIndex = -1;

        if (player == null || !PhotonNetwork.InRoom)
        {
            return false;
        }

        for (int index = 0; index < SlotCount; index++)
        {
            if (GetActorNumberAtSlot(index) != player.ActorNumber)
            {
                continue;
            }

            slotIndex = index;
            return true;
        }

        return false;
    }


    // Quando entriamo nella Room, il MasterClient ricostruisce le assegnazioni degli slot e notifica l'evento SlotsChanged
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            RebuildSlotAssignmentsAsMaster();
        }

        PublishSlotsChanged();
    }


    // Quando entra un nuovo player nella Room, il MasterClient gli assegna il primo slot disponibile
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        Debug.Log($"[PhotonRoomSlotManager] Richiesta assegnazione slot per " + $"Actor {newPlayer.ActorNumber}.");

        RebuildSlotAssignmentsAsMaster();
    }


    // Quando un player lascia la Room, il MasterClient libera lo slot occupato da quel player
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        Debug.Log($"[PhotonRoomSlotManager] Liberazione slot di " + $"Actor {otherPlayer.ActorNumber}.");

        RebuildSlotAssignmentsAsMaster();
    }


    // Se cambia il MasterClient, il nuovo Master verifica e ricostruisce le assegnazioni degli slot
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (newMasterClient == null || !newMasterClient.IsLocal)
        {
            return;
        }

        Debug.Log("[PhotonRoomSlotManager] Il client locale è il nuovo Master. " + "Verifica degli slot...");

        RebuildSlotAssignmentsAsMaster();
    }

    
    // Chiamata quando cambiano le Room Custom Properties 
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (!ContainsSlotProperty(propertiesThatChanged))
        {
            return;
        }

        PublishSlotsChanged();
    }

    // Quando lasciamo la Room notifichiamo l'UI
    public override void OnLeftRoom()
    {
        SlotsChanged?.Invoke();
    }



    // Restituisce le assegnazioni conservando gli slot validi.
    // Es:
    // slot_0 -> Player 1
    // slot_1 -> libero
    // slot_2 -> Player 3
    // 
    // Se sentra Player 4 gli da lo slot 1.
    private void RebuildSlotAssignmentsAsMaster()
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        Dictionary<int, Player> playersByActorNumber = new Dictionary<int, Player>();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player == null)
            {
                continue;
            }

            playersByActorNumber[player.ActorNumber] = player;
        }

        HashSet<int> assignedActorNumbers = new HashSet<int>();

        Queue<int> availableSlots = new Queue<int>();

        Hashtable propertiesToUpdate = new Hashtable();


        // Prima conserviamo tutte le assegnazioni ancora valide
        // Uno slot viene considerato non valido quando è vuoto, appartiane ad un player che ha lasciato la Room 
        // o quando lo stesso ActorNumber compare in più slot
        for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
        {
            int actorNumber = GetActorNumberAtSlot(slotIndex);

            bool assignmentIsValid = actorNumber > 0 && playersByActorNumber.ContainsKey(actorNumber) && assignedActorNumbers.Add(actorNumber);

            if (assignmentIsValid)
            {
                continue;
            }

            availableSlots.Enqueue(slotIndex);

            if (actorNumber != 0)
            {
                propertiesToUpdate[GetSlotKey(slotIndex)] = 0;
            }
        }



        // Poi assegniamo un posto ai player che non ne possiedono
        List<Player> orderedPlayers = new List<Player>(playersByActorNumber.Values);

        orderedPlayers.Sort((firstPlayer, secondPlayer) => firstPlayer.ActorNumber.CompareTo(secondPlayer.ActorNumber));

        foreach (Player player in orderedPlayers)
        {
            if (assignedActorNumbers.Contains(player.ActorNumber))
            {
                continue;
            }

            if (availableSlots.Count == 0)
            {
                Debug.LogWarning($"[PhotonRoomSlotManager] Nessuno slot disponibile " + $"per Actor {player.ActorNumber}.");

                break;
            }

            int assignedSlot = availableSlots.Dequeue();

            propertiesToUpdate[GetSlotKey(assignedSlot)] = player.ActorNumber;

            assignedActorNumbers.Add(player.ActorNumber);

            Debug.Log($"[PhotonRoomSlotManager] Actor " + $"{player.ActorNumber} assegnato allo Slot " + $"{assignedSlot}.");
        }

        if (propertiesToUpdate.Count == 0)
        {
            PublishSlotsChanged();
            return;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(propertiesToUpdate);
    }


    // Scrivile nella Console la configurazione corrente e notifica gli eventuali listener
    private void PublishSlotsChanged()
    {
        if (!PhotonNetwork.InRoom)
        {
            SlotsChanged?.Invoke();
            return;
        }

        List<string> slotDescriptions = new List<string>();

        for (int slotIndex = 0; slotIndex < SlotCount; slotIndex++)
        {
            int actorNumber = GetActorNumberAtSlot(slotIndex);

            if (actorNumber == 0)
            {
                slotDescriptions.Add($"Slot {slotIndex}: libero");

                continue;
            }

            Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

            string playerName = GetDisplayName(player, actorNumber);

            slotDescriptions.Add($"Slot {slotIndex}: {playerName} " + $"(Actor {actorNumber})");
        }

        Debug.Log("[PhotonRoomSlotManager] " + string.Join(" | ", slotDescriptions));

        SlotsChanged?.Invoke();
    }

    // Restituisce il nome visualizzato del player, oppure "Player {actorNumber}" se il player non esiste o non ha un NickName valido
    private static string GetDisplayName(Player player, int actorNumber)
    {
        if (player != null && !string.IsNullOrWhiteSpace(player.NickName))
        {
            return player.NickName;
        }

        return $"Player {actorNumber}";
    }

    // Restituisce la chiave della proprietà della Room per uno specifico slot
    private static string GetSlotKey(int slotIndex)
    {
        return $"{SlotKeyPrefix}{slotIndex}";
    }


    // Controlla se l'indice dello slot è valido (tra 0 e SlotCount - 1)
    private static bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < SlotCount;
    }


    // Controlla se le proprietà contengono almeno una chiave che inizia con "slot_"
    private static bool ContainsSlotProperty(Hashtable properties)
    {
        if (properties == null)
        {
            return false;
        }

        foreach (object rawKey in properties.Keys)
        {
            if (rawKey is string key && key.StartsWith(SlotKeyPrefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }


}
