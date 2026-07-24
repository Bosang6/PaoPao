using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;



/*
 * Gestisce il pulsante PLAY nel RoomLobbyPanel
 *
 * Responsabilità:
 * - permettere l'avvio soltanto al Master Client
 * - controllare che tutti i giocatori umani siano Ready
 * - aggiornarsi quando un player entra, esce o cambia Ready
 * - aggiornarsi quando cambia il Master Client
 */



public sealed class UIRoomPlayManager : MonoBehaviourPunCallbacks
{

    [Header("Play Button")]
    [Tooltip("Pulsante utilizzato dal Master Client per avviare la partita.")]
    [SerializeField] private Button playButton;

    // 
    public override void OnEnable()
    {
        base.OnEnable();

        if (playButton != null)
        {
            playButton.onClick.AddListener(HandlePlayButtonClicked);
        }

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
        bool canStartGame = PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && AreAllPlayersReady();

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
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[UIRoomPlayManager] " + "Impossibile avviare la partita: " + "il client non è dentro una Room.", this);

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

        Debug.Log("[UIRoomPlayManager] " + "Tutti i giocatori sono Ready. " + "La partita può essere avviata."
        );
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


    // Aggiorna il pulsante quando un nuovo player entra.
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
        RefreshPlayButton();
    }


    public override void OnJoinedRoom()
    {
        RefreshPlayButton();
    }


    public override void OnLeftRoom()
    {
        if (playButton != null)
        {
            playButton.interactable = false;
        }
    }






}
