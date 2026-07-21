using Photon.Pun;
using UnityEngine;


/*
 * Gestisce l'ingresso del client nella Lobby Photon
 * 
 * Responsabilità:
 * - aspettare che la connessione al Master sia pronta
 * - entrare automaticamente nella lobby predefinita
 * - notificare tramite log esito dell'operazione 
 * 
 */

public sealed class PhotonLobbyManager : MonoBehaviourPunCallbacks
{

    [Header("Lobby Settings")]

    [Tooltip("Se attivo, il client entra automaticamente nella Lobby " + "quando raggiunge il Master Server.")]
    [SerializeField] private bool joinLobbyAutomatically = true;

    // Indica se il client si trova attualmente nella Lobby Photon
    public bool IsInLobby => PhotonNetwork.InLobby;



    private void Start()
    {   
        // 
        if (joinLobbyAutomatically && PhotonNetwork.IsConnectedAndReady)
        {
            JoinLobby();
        }
    }

    // Invia a Photon la richiesta di ingresso nella Lobby predefinita
    public void JoinLobby()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("[PhotonLobbyManager] Impossibile entrare nella Lobby: " + "il client non è ancora connesso al Master Server.");

            return;
        }

        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[PhotonLobbyManager] Impossibile entrare nella Lobby: " + "il client si trova già dentro una Room.");

            return;
        }

        if (PhotonNetwork.InLobby)
        {
            Debug.Log("[PhotonLobbyManager] Il client è già nella Lobby.");

            return;
        }

        Debug.Log("[PhotonLobbyManager] Richiesta di ingresso nella Lobby...");

        bool requestSent = PhotonNetwork.JoinLobby();

        if (!requestSent)
        {
            Debug.LogError("[PhotonLobbyManager] Photon non ha accettato " + "la richiesta di ingresso nella Lobby.");
        }
    }


    // Chiamata da Photon quando il client raggiunge il Master Server
    public override void OnConnectedToMaster()
    {
        if (joinLobbyAutomatically)
        {
            JoinLobby();
        }
    }

    // Chiamata quando il client entra correttamente nella lobby
    public override void OnJoinedLobby()
    {
        Debug.Log("[PhotonLobbyManager] Entrato correttamente nella Lobby Photon.");
    }


    // Chiamata quando il client lascia la lobby
    public override void OnLeftLobby()
    {
        Debug.Log("[PhotonLobbyManager] Il client ha lasciato la Lobby Photon." );
    }


}
