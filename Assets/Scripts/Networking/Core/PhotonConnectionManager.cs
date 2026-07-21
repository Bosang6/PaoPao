using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/*
 * Gestisce la connessione principale al Photon Cloud
 * 
 * Questo oggetto viene creato nel TitleMenu, rimane attivo durante i cambi di scena, evita
 * la presenza di copie duplicate ed espone in futuro la connessione agli altri sistemi 
 * networking.
 */


public sealed class PhotonConnectionManager : MonoBehaviourPunCallbacks
{

    public static PhotonConnectionManager Instance { get; private set; }

    [Header("Connection Settings")]

    [Tooltip("Client con versioni differenti non verranno inseriti nelle stesse Room.")]
    [SerializeField]
    private string gameVersion = "0.1";

    [Tooltip("Avvia automaticamente la connessione quando viene caricata la scena.")]
    [SerializeField]
    private bool connectOnStart = true;

    // Indica se il client è collegato e pronto per usare il matchmaking
    public bool IsConnectedAndReady => PhotonNetwork.IsConnectedAndReady;

    private void Awake()
    {
        // Singleton: deve esserci una sola istanza nel manager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Non verrà distrutta nel cambio scena
        DontDestroyOnLoad(gameObject);

        // 
        PhotonNetwork.AutomaticallySyncScene = true;
    }


    private void Start()
    {
        if (connectOnStart)
        {
            Connect();
        }
    }

    // Avvia la connessione usando le impostazioni di PhotonServerSettings
    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("[PhotonConnectionManager] Il client è già connesso a Photon.");
            return;
        }

        PhotonNetwork.GameVersion = gameVersion;

        Debug.Log( $"[PhotonConnectionManager] Connessione a Photon avviata. " + $"Game Version: {gameVersion}" );

        bool connectionStarted = PhotonNetwork.ConnectUsingSettings();

        if (!connectionStarted)
        {
            Debug.LogError("[PhotonConnectionManager] Impossibile avviare la connessione.");
        }
    }


    // Permette di disconnettersi da Photon, se il client è connesso
    public void Disconnect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }

        PhotonNetwork.Disconnect();
    }


    // Chiamata da PUN quando il client raggiunge il Master Server ed è pronto per lobby e matchmaking
    public override void OnConnectedToMaster()
    {
        Debug.Log($"[PhotonConnectionManager] Connesso al Master Server. " + $"Regione: {PhotonNetwork.CloudRegion}" );
    }


    // Chiamata da PUN quando il client viene disconnesso da Photon
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"[PhotonConnectionManager] Disconnesso da Photon. " + $"Causa: {cause}" );
    }


}
