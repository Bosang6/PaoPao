using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;


/*
 * Mantiene una cache locale delle Room visibili nella Lobby Photon
 * 
 * Questo manager aggiorna la cache, rimuove Room non più disponibili,
 * espone una lista ordinata di elementi UI, notifica i listener quando la lista cambia
 * 
 */

public sealed class PhotonRoomListManager : MonoBehaviourPunCallbacks
{
    private readonly Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>(StringComparer.Ordinal);

    private IReadOnlyList<RoomInfo> currentRooms = Array.Empty<RoomInfo>();

    [Header("Debug")]

    [Tooltip("Scrive nella Console la lista delle Room a ogni aggiornamento.")]
    [SerializeField]
    private bool logRoomListUpdates = true;

    // Snapshot aggiornato delle Room attualmente presenti nella cache
    public IReadOnlyList<RoomInfo> CurrentRooms => currentRooms;

    // Evento richiamato ogni volta che la lista delle Room cambia
    public event Action<IReadOnlyList<RoomInfo>> RoomListChanged;

    // Cerca una Room nella cache usando il suo nome
    public bool TryGetRoom(string roomName, out RoomInfo roomInfo)
    {
        if (string.IsNullOrWhiteSpace(roomName))
        {
            roomInfo = null;
            return false;
        }

        return cachedRoomList.TryGetValue(roomName, out roomInfo);
    }

    // Quando entriamo in una lobby, azzeriamo eventuali dati appartenenti ad una lobby precedente
    public override void OnJoinedLobby()
    {
        ClearRoomList("[PhotonRoomListManager] Entrato nella Lobby: cache azzerata.");
    }


    // Photon richiama questo metodo ogni volta che una o piu Room vengono aggiornate,
    // cambiano stato, cambiano numero di giocatori, vengono create o rimosse
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo roomInfo in roomList)
        {
            if (roomInfo == null)
            {
                continue;
            }

            if (roomInfo.RemovedFromList)
            {
                cachedRoomList.Remove(roomInfo.Name);
                continue;
            }

            cachedRoomList[roomInfo.Name] = roomInfo;
        }

        PublishRoomList();
    }


    // Quando lasciamo la lobby la lista non è più valida
    public override void OnLeftLobby()
    {
        ClearRoomList("[PhotonRoomListManager] Lobby lasciata: cache azzerata.");
    }


    // Quando perdiamo la connessione a Photon, i dati della lobby non sono più validi
    public override void OnDisconnected(DisconnectCause cause)
    {
        ClearRoomList($"[PhotonRoomListManager] Disconnesso da Photon. " + $"Cache azzerata. Causa: {cause}.");
    }


    // Ricostruisce uno snapshot ordinato della cache e notifica gli eventuali listener
    private void PublishRoomList()
    {
        List<RoomInfo> snapshot = new List<RoomInfo>(cachedRoomList.Values);

        snapshot.Sort( (firstRoom, secondRoom) => string.Compare(firstRoom.Name, secondRoom.Name, StringComparison.Ordinal) );

        currentRooms = snapshot.AsReadOnly();

        RoomListChanged?.Invoke(currentRooms);

        if (!logRoomListUpdates)
        {
            return;
        }

        Debug.Log($"[PhotonRoomListManager] Room disponibili: " + $"{currentRooms.Count}." );

        foreach (RoomInfo roomInfo in currentRooms)
        {
            bool hasFreeSlot = roomInfo.MaxPlayers == 0 || roomInfo.PlayerCount < roomInfo.MaxPlayers;

            bool isJoinable = roomInfo.IsOpen && hasFreeSlot;

            Debug.Log( $"[PhotonRoomListManager] Room: '{roomInfo.Name}' | " +
                $"Giocatori: {roomInfo.PlayerCount}/" +
                $"{roomInfo.MaxPlayers} | " +
                $"Aperta: {roomInfo.IsOpen} | " +
                $"Join possibile: {isJoinable}"
            );
        }
    }


    // Elimina tutti i dati presenti nella cache e notifica la futura UI
    private void ClearRoomList(string debugMessage)
    {
        cachedRoomList.Clear();
        currentRooms = Array.Empty<RoomInfo>();

        RoomListChanged?.Invoke(currentRooms);

        if (logRoomListUpdates)
        {
            Debug.Log(debugMessage);
        }
    }


}
