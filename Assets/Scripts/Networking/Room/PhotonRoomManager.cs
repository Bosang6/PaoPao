using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/*
 * Gestisce la creazione, l'ingresso e l'uscita dalle Room Photon.
 * 
 * 
 * 
 * 
 * 
 */


public sealed class PhotonRoomManager : MonoBehaviourPunCallbacks
{
    [Header("Room Settings")]

    [Tooltip("Numero massimo di giocatori umani ammessi nella Room.")]
    [SerializeField] private byte maxPlayers = 4;

    [Tooltip("Prefisso utilizzato per creare Room temporanee di test.")]
    [SerializeField] private string testRoomNamePrefix = "PaoPao-Test";

    // Indica se il client locale si trova attualmente in una Room Photon
    public bool IsInRoom => PhotonNetwork.InRoom;

    // Indica se il client locale è il Master Client della Room Photon corrente
    public bool IsMasterClient => PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient;


    // Crea una nuova Room Photon. Il client che la crea entrerà automaticamente nella Room appena creata.
    public void CreateRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("[PhotonRoomManager] Impossibile creare la Room: " + "il client non è ancora connesso al Master Server." );

            return;
        }

        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning( "[PhotonRoomManager] Impossibile creare la Room: " + "il client si trova già dentro una Room." );

            return;
        }

        string validatedRoomName = roomName?.Trim();

        if (string.IsNullOrWhiteSpace(validatedRoomName))
        {
            Debug.LogWarning( "[PhotonRoomManager] Impossibile creare la Room: " + "il nome non è valido." );

            return;
        }

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = maxPlayers,
            IsOpen = true,
            IsVisible = true,

            // Quando un player lascia la Room, gli evente e gli oggetti memorizzati nella sua cache vengono rimossi.
            CleanupCacheOnLeave = true
        };

        Debug.Log( $"[PhotonRoomManager] Creazione Room '{validatedRoomName}' " + $"con massimo {maxPlayers} giocatori..." );

        bool requestSent = PhotonNetwork.CreateRoom( validatedRoomName, roomOptions, TypedLobby.Default);

        if (!requestSent)
        {
            Debug.LogError("[PhotonRoomManager] Photon non ha accettato " + "la richiesta di creazione della Room." );
        }
    }


    // Metodo temporaneo per creare una Room di test con un nome casuale. Utile per il debug e i test in fase di sviluppo
    [ContextMenu("Create Test Room")]
    private void CreateTestRoom()
    {
        string randomSuffix = Random.Range(1000, 10000).ToString();
        string roomName = $"{testRoomNamePrefix}-{randomSuffix}";

        CreateRoom(roomName);
    }


    // Chiamata solamente sul client che ha creato la Room
    public override void OnCreatedRoom()
    {
        Debug.Log( $"[PhotonRoomManager] Room creata correttamente: " + $"'{PhotonNetwork.CurrentRoom.Name}'." );
    }


    // Chiamata quando il client entra correttamente nella Room
    public override void OnJoinedRoom()
    {
        Debug.Log(
            $"[PhotonRoomManager] Entrato nella Room " +
            $"'{PhotonNetwork.CurrentRoom.Name}'. " +
            $"Giocatori: {PhotonNetwork.CurrentRoom.PlayerCount}/" +
            $"{PhotonNetwork.CurrentRoom.MaxPlayers}. " +
            $"Master Client: {PhotonNetwork.IsMasterClient}."
        );
    }


    // Chiamata quando la creazione della Room fallisce
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError( $"[PhotonRoomManager] Creazione Room fallita. " + $"Codice: {returnCode}. Messaggio: {message}" );
    }


    // Quando l'ingresso in una Room esistente fallisce
    public override void OnJoinRoomFailed( short returnCode, string message)
    {
        Debug.LogError( $"[PhotonRoomManager] Ingresso nella Room fallito. " + $"Codice: {returnCode}. Messaggio: {message}" );
    }

    // Quando il client lascia la Room 
    public override void OnLeftRoom()
    {
        Debug.Log( "[PhotonRoomManager] Il client ha lasciato la Room." );
    }

    // Entra in una Room esistente tramite il suo nome
    public void JoinRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning( "[PhotonRoomManager] Impossibile entrare nella Room: " + "il client non è connesso al Master Server.");

            return;
        }

        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[PhotonRoomManager] Il client si trova già dentro una Room.");

            return;
        }

        string validatedRoomName = roomName?.Trim();

        if (string.IsNullOrWhiteSpace(validatedRoomName))
        {
            Debug.LogWarning("[PhotonRoomManager] Il nome della Room non è valido.");

            return;
        }

        Debug.Log($"[PhotonRoomManager] Tentativo di ingresso nella Room " + $"'{validatedRoomName}'...");

        bool requestSent = PhotonNetwork.JoinRoom(validatedRoomName);

        if (!requestSent)
        {
            Debug.LogError("[PhotonRoomManager] Photon non ha accettato " + "la richiesta di ingresso nella Room.");
        }
    }




}
