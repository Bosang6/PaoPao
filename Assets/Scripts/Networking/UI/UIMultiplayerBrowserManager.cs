using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * Gestisce l'interfaccia del browser multiplayer
 * 
 * Responsabilità:
 * - mostrare le Room presenti nella cache Photon
 * - creare dinamicamente i RoomListItem 
 * - creare una Room tramite il pulsante Host
 * - entrare in una Room tramite il pulsante Join
 * - aggiornare manualmente la UI tramite il pulsante Refresh 
 * - passare dal browser alla lobby interna della Room
 * 
 */


public sealed class UIMultiplayerBrowserManager : MonoBehaviourPunCallbacks
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject multiplayerBrowserPanel;
    [SerializeField] private GameObject roomLobbyPanel;

    [Header("Room List")]
    [SerializeField] private Transform roomListContent;
    [SerializeField] private RoomListItemUI roomListItemPrefab;
    [SerializeField] private GameObject noRoomsText;

    [Header("Buttons")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button backButton;

    [Header("Feedback")]
    [SerializeField] private TMP_Text statusText;

    [Header("Host Settings")]
    [Tooltip("Prefisso usato per generare il nome delle Room.")]
    [SerializeField] private string roomNamePrefix = "PaoPao";

    private readonly List<RoomListItemUI> spawnedRoomItems = new List<RoomListItemUI>();

    private PhotonRoomManager roomManager;
    private PhotonRoomListManager roomListManager;

    private bool isBusy;


    public override void OnEnable()
    {
        base.OnEnable();

        if (!ResolveNetworkingManagers())
        {
            UpdateInteractionState();
            return;
        }

        roomListManager.RoomListChanged += HandleRoomListChanged;

        if (hostButton != null)
        {
            hostButton.onClick.AddListener(HandleHostButtonClicked);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(HandleRefreshButtonClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(HandleBackButtonClicked);
        }

        RebuildRoomList(roomListManager.CurrentRooms);
        RefreshStatus();
        UpdateInteractionState();
    }


    public override void OnDisable()
    {
        if (roomListManager != null)
        {
            roomListManager.RoomListChanged -= HandleRoomListChanged;
        }

        if (hostButton != null)
        {
            hostButton.onClick.RemoveListener(HandleHostButtonClicked);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.RemoveListener(HandleRefreshButtonClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackButtonClicked);
        }

        base.OnDisable();
    }

    // Può essere collegato al pulsante Multiplayer del MainMenu
    public void OpenBrowser()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (roomLobbyPanel != null)
        {
            roomLobbyPanel.SetActive(false);
        }

        if (multiplayerBrowserPanel != null)
        {
            multiplayerBrowserPanel.SetActive(true);
        }

        if (roomListManager != null)
        {
            RebuildRoomList(roomListManager.CurrentRooms);
        }

        RefreshStatus();
        UpdateInteractionState();
    }


    // Risolve i riferimenti ai manager di networking Photon
    private bool ResolveNetworkingManagers()
    {
        PhotonConnectionManager connectionManager = PhotonConnectionManager.Instance;

        // Fallback utile quando la scena MainMenu viene avviata direttamente nell'Editor
        if (connectionManager == null)
        {
            connectionManager = FindFirstObjectByType<PhotonConnectionManager>();
        }

        if (connectionManager == null)
        {
            Debug.LogError("[UIMultiplayerBrowserManager] " + "PhotonConnectionManager non trovato.");

            SetStatus("Networking non disponibile.");
            return false;
        }

        roomManager = connectionManager.GetComponent<PhotonRoomManager>();

        roomListManager = connectionManager.GetComponent<PhotonRoomListManager>();

        if (roomManager == null)
        {
            Debug.LogError("[UIMultiplayerBrowserManager] " + "PhotonRoomManager non trovato sul GameObject Networking.");

            return false;
        }

        if (roomListManager == null)
        {
            Debug.LogError("[UIMultiplayerBrowserManager] " + "PhotonRoomListManager non trovato sul GameObject Networking.");

            return false;
        }

        return true;
    }

    // Richiamato quando la lista delle Room cambia
    private void HandleRoomListChanged(IReadOnlyList<RoomInfo> rooms) 
    {
        RebuildRoomList(rooms);
        RefreshStatus();
    }

    // Ricostruisce la lista delle Room nella UI
    private void RebuildRoomList(IReadOnlyList<RoomInfo> rooms)
    {
        ClearSpawnedRoomItems();

        if (roomListContent == null || roomListItemPrefab == null)
        {
            Debug.LogError("[UIMultiplayerBrowserManager] " + "Content o prefab RoomListItem non assegnato.");

            return;
        }

        if (rooms != null)
        {
            foreach (RoomInfo roomInfo in rooms)
            {
                if (roomInfo == null)
                {
                    continue;
                }

                RoomListItemUI roomItem = Instantiate(roomListItemPrefab, roomListContent);

                roomItem.Initialize( roomInfo, HandleJoinRequested);

                spawnedRoomItems.Add(roomItem);
            }
        }

        bool roomListIsEmpty = spawnedRoomItems.Count == 0;

        if (noRoomsText != null)
        {
            noRoomsText.SetActive(roomListIsEmpty);
        }

        UpdateInteractionState();
    }


    // Distrugge tutti i RoomListItem spawnati in precedenza
    private void ClearSpawnedRoomItems()
    {
        foreach (RoomListItemUI roomItem in spawnedRoomItems)
        {
            if (roomItem != null)
            {
                Destroy(roomItem.gameObject);
            }
        }

        spawnedRoomItems.Clear();
    }


    // Richiamato quando l'utente clicca il pulsante Host
    private void HandleHostButtonClicked()
    {
        if (roomManager == null || isBusy)
        {
            return;
        }

        string randomIdentifier =
            Guid.NewGuid()
                .ToString("N")
                .Substring(0, 6)
                .ToUpperInvariant();

        string roomName = $"{roomNamePrefix}-{randomIdentifier}";

        SetBusy(true, $"Creazione Room {roomName}...");

        roomManager.CreateRoom(roomName);
    }


    // Richiamato quando l'utente clicca il pulsante Join su un RoomListItem
    private void HandleJoinRequested(string roomName)
    {
        if (roomManager == null || isBusy)
        {
            return;
        }

        SetBusy(true, $"Ingresso in {roomName}...");

        roomManager.JoinRoom(roomName);
    }

    // Richiamato quando l'utente clicca il pulsante Refresh
    private void HandleRefreshButtonClicked()
    {
        if (roomListManager == null || isBusy)
        {
            return;
        }

        // Photon aggiorna automaticamente la cache delle Room
        RebuildRoomList(roomListManager.CurrentRooms);
        RefreshStatus();
    }

    // Richiamato quando l'utente clicca il pulsante Back
    private void HandleBackButtonClicked()
    {
        if (isBusy)
        {
            return;
        }

        if (multiplayerBrowserPanel != null)
        {
            multiplayerBrowserPanel.SetActive(false);
        }

        if (roomLobbyPanel != null)
        {
            roomLobbyPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    // Imposta lo stato di busy e aggiorna la UI di conseguenza
    private void SetBusy(bool busy, string message = null)
    {
        isBusy = busy;

        if (!string.IsNullOrWhiteSpace(message))
        {
            SetStatus(message);
        }

        UpdateInteractionState();
    }


    // Aggiorna lo stato di interazione dei pulsanti e dei RoomListItem
    private void UpdateInteractionState()
    {
        bool canUseRoomBrowser = !isBusy && PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InLobby && !PhotonNetwork.InRoom;

        if (hostButton != null)
        {
            hostButton.interactable = canUseRoomBrowser;
        }

        if (refreshButton != null)
        {
            refreshButton.interactable = canUseRoomBrowser;
        }

        if (backButton != null)
        {
            backButton.interactable = !isBusy;
        }

        foreach (RoomListItemUI roomItem in spawnedRoomItems)
        {
            if (roomItem != null)
            {
                roomItem.SetInteractionEnabled(canUseRoomBrowser);
            }
        }
    }

    // Aggiorna il messaggio di stato nella UI in base alla connessione e alla lobby
    private void RefreshStatus()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SetStatus("Connessione a Photon...");
            return;
        }

        if (PhotonNetwork.InRoom)
        {
            SetStatus($"Room: {PhotonNetwork.CurrentRoom.Name}");

            return;
        }

        if (!PhotonNetwork.InLobby)
        {
            SetStatus("Ingresso nella Lobby...");
            return;
        }

        int roomCount = roomListManager != null ? roomListManager.CurrentRooms.Count : 0;

        SetStatus(roomCount == 1 ? "1 Room disponibile" : $"{roomCount} Room disponibili"
        );
    }

    // Imposta il messaggio di stato nella UI
    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    // Callback di Photon
    public override void OnConnectedToMaster()
    {
        RefreshStatus();
        UpdateInteractionState();
    }

    // Callback di Photon
    public override void OnJoinedLobby()
    {
        SetBusy(false);

        if (roomListManager != null)
        {
            RebuildRoomList(roomListManager.CurrentRooms);
        }

        RefreshStatus();
    }

    // Callback di Photon
    public override void OnJoinedRoom()
    {
        SetBusy(false);

        Debug.Log($"[UIMultiplayerBrowserManager] " + $"Apertura Room Lobby: " + $"'{PhotonNetwork.CurrentRoom.Name}'.");

        if (multiplayerBrowserPanel != null)
        {
            multiplayerBrowserPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (roomLobbyPanel != null)
        {
            roomLobbyPanel.SetActive(true);
        }
    }

    // Callback di Photon
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetBusy(false);

        SetStatus($"Creazione Room fallita: {message}");
    }

    // Callback di Photon
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetBusy(false);

        SetStatus($"Ingresso nella Room fallito: {message}");
    }

    // Callback di Photon
    public override void OnLeftRoom()
    {
        SetBusy(false);

        if (roomLobbyPanel != null)
        {
            roomLobbyPanel.SetActive(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (multiplayerBrowserPanel != null)
        {
            multiplayerBrowserPanel.SetActive(true);
        }

        RefreshStatus();
    }

    // Callback di Photon
    public override void OnDisconnected(DisconnectCause cause)
    {
        SetBusy(false);

        SetStatus($"Disconnesso: {cause}");
    }

}
