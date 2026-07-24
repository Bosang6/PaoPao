using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;


/*
 * Gestisce il pulsante PLAY nel RoomLobbyPanel.
 *
 * Responsabilità:
 * - permettere l'avvio soltanto al Master Client;
 * - controllare che tutti i giocatori umani siano Ready;
 * - disabilitare il pulsante sui Client;
 * - chiudere la Room prima dell'avvio;
 * - caricare la scena di gioco in maniera sincronizzata;
 * - aggiornarsi quando un player entra, esce o cambia Ready;
 * - aggiornarsi quando cambia il Master Client.
 */



public sealed class UIRoomPlayManager : MonoBehaviourPunCallbacks
{

    [Header("Play Button")]
    [Tooltip("Pulsante utilizzato dal Master Client " + "per avviare la partita.")]
    [SerializeField] private Button playButton;

    [Header("Game Scene")]
    [Tooltip("Nome della scena di gioco che verrà caricata dal Master Client.")]
    [SerializeField] private string gameSceneName = "GameScene";


    // Impedisce richieste di avvio multiple
    private bool isStartingGame;

    // 
    public override void OnEnable()
    {
        base.OnEnable();

        if (playButton != null)
        {
            playButton.onClick.AddListener(HandlePlayButtonClicked);
        }

        isStartingGame = false;

        RefreshPlayButton();
    }

    // 
    public override void OnDisable()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(HandlePlayButtonClicked);
        }

        base.OnDisable();
    }


    // Il Play può essere utilizzato soltanto dal Master Client quando tutti i giocatori presenti sono Ready
    private void RefreshPlayButton()
    {
        bool canStartGame = PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && AreAllPlayersReady() && !isStartingGame;

        if (playButton != null)
        {
            playButton.interactable = canStartGame;
        }
    }


    // Controlla lo stato Ready di tutti i player umani attualmente presenti nella Room
    private bool AreAllPlayersReady()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            return false;
        }

        Player[] players = PhotonNetwork.PlayerList;

        if (players == null || players.Length == 0)
        {
            return false;
        }

        foreach (Player player in players)
        {
            bool hasReadyProperty = PhotonPlayerProperties.TryGetReady(player, out bool isReady);

            if (!hasReadyProperty || !isReady)
            {
                return false;
            }
        }

        return true;
    }

    // Per ora verifica solamente le condizioni. L'avvio sincro verrà implementeato succ
    private void HandlePlayButtonClicked()
    {

        if (isStartingGame)
        {
            return;
        }

        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogWarning("[UIRoomPlayManager] " + "Impossibile avviare la partita: il client non è dentro una Room.", this);
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[UIRoomPlayManager] " + "Soltanto il Master Client può avviare la partita.", this);

            return;
        }

        if (!AreAllPlayersReady())
        {
            Debug.LogWarning("[UIRoomPlayManager] " + "Impossibile avviare la partita: non tutti i giocatori sono Ready.", this);

            RefreshPlayButton();
            return;
        }

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogError("[UIRoomPlayManager] " + "Il nome della scena di gioco non è valido.", this);
            return;
        }

        isStartingGame = true;

        if (playButton != null)
        {
            playButton.interactable = false;
        }


        // Impedisce l'ingresso di nuovi giocatori durante il caricamento della partita.
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        
        Debug.Log("[UIRoomPlayManager] " + $"Avvio sincronizzato della scena " + $"'{gameSceneName}'.");

        // Deve essere chiamato soltanto dal Master Client.
        // Con AutomaticallySyncScene attivo, tutti gli altri giocatori verranno portati nella stessa scena
        PhotonNetwork.LoadLevel(gameSceneName);
    }


    // Aggiorna il pulsante quando cambia lo stato Ready di uno dei giocatori.
    public override void OnPlayerPropertiesUpdate( Player targetPlayer, Hashtable changedProperties)
    {
        if (changedProperties == null || !changedProperties.ContainsKey(PhotonPlayerProperties.ReadyKey))
        {
            return;
        }

        RefreshPlayButton();
    }


    // Aggiorna il pulsante quando un nuovo player entra. Il player entra sempre NOT READY
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshPlayButton();
    }


    // Aggiorna il pulsante quando un player esce.
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshPlayButton();
    }



    // Aggiorna il pulsante quando cambia il Master Client.
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        isStartingGame = false;
        RefreshPlayButton();
    }


    public override void OnJoinedRoom()
    {
        isStartingGame = false;
        RefreshPlayButton();
    }


    public override void OnLeftRoom()
    {
        isStartingGame = false;

        if (playButton != null)
        {
            playButton.interactable = false;
        }
    }






}
