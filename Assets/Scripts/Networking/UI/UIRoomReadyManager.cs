using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/*
 * Gestisce il pulsante Ready del giocatore locale
 * 
 * Responsabilità:
 * - leggere lo stato Ready locale
 * - alternare READY / NOT READY
 * - salvare lo stato nelle Player Custom Properties
 * - aggiornare il testo del pulsante
 * - impedire click ripetuti mentre attendiamo Photon
 * 
 */

public sealed class UIRoomReadyManager : MonoBehaviourPunCallbacks
{
    [Header("Ready Button")]
    [Tooltip("Pulsante usato per cambiare lo stato Ready.")]
    [SerializeField] private Button readyButton;
    [Tooltip("Testo mostrato dentro il pulsante Ready.")]
    [SerializeField] private TMP_Text readyButtonText;

    [Header("Button Labels")]
    [SerializeField] private string readyLabel = "READY";
    [SerializeField] private string cancelReadyLabel = "CANCEL READY";

    private bool requestPending;


    // Quando il pannello viene attivato:
    // - registra le callback Photon;
    // - collega il pulsante;
    // - legge lo stato locale corrente.
    public override void OnEnable()
    {
        base.OnEnable();

        if (readyButton != null)
        {
            readyButton.onClick.AddListener(HandleReadyButtonClicked);
        }

        requestPending = false;
        RefreshReadyButton();
    }

    // Rimuove il listener quando il pannello viene disattivato
    public override void OnDisable()
    {
        if (readyButton != null)
        {
            readyButton.onClick.RemoveListener(HandleReadyButtonClicked);
        }

        requestPending = false;
        base.OnDisable();
    }


    // Viene chiamato quando il giocatore preme il pulsante Ready.
    private void HandleReadyButtonClicked()
    {
        if (requestPending)
        {
            return;
        }

        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
        {
            Debug.LogWarning("[UIRoomReadyManager] Impossibile cambiare " + "lo stato Ready: il client non è in una Room.", this);

            return;
        }

        bool currentReadyState = PhotonPlayerProperties.TryGetReady(PhotonNetwork.LocalPlayer, out bool storedReadyState) && storedReadyState;

        bool requestedReadyState = !currentReadyState;

        requestPending = true;

        if (readyButton != null)
        {
            readyButton.interactable = false;
        }

        bool requestAccepted = PhotonPlayerProperties.SetReady(PhotonNetwork.LocalPlayer, requestedReadyState);

        if (!requestAccepted)
        {
            Debug.LogError("[UIRoomReadyManager] Photon non ha accettato " + "la modifica dello stato Ready.", this);

            requestPending = false;
            RefreshReadyButton();
            return;
        }

        Debug.Log("[UIRoomReadyManager] " + $"Actor {PhotonNetwork.LocalPlayer.ActorNumber} " + $"ha richiesto Ready = {requestedReadyState}.");
    }

    // Callback ricevuta quando cambiano le Player Custom Properties
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProperties)
    {
        if (
            targetPlayer == null ||
            !targetPlayer.IsLocal ||
            changedProperties == null ||
            !changedProperties.ContainsKey(
                PhotonPlayerProperties.ReadyKey
            )
        )
        {
            return;
        }

        requestPending = false;

        RefreshReadyButton();
    }


    // Legge lo stato Ready locale e aggiorna testo ed interazione del pulsante
    private void RefreshReadyButton()
    {
        bool canUseReadyButton = PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer != null && !requestPending;

        
        bool isReady = PhotonNetwork.LocalPlayer != null && PhotonPlayerProperties.TryGetReady(PhotonNetwork.LocalPlayer, out bool storedReadyState) && storedReadyState;

        if (readyButton != null)
        {
            readyButton.interactable = canUseReadyButton;
        }

        if (readyButtonText != null)
        {
            readyButtonText.text = isReady ? cancelReadyLabel : readyLabel;
        }
    }

    // Se il pannello fosse già attivo durante l'ingresso nella Room, aggiorna il pulsante
    public override void OnJoinedRoom()
    {
        requestPending = false;

        RefreshReadyButton();
    }


    // Quando il client lascia la Room, disattiva il pulsante e ripristina il testo
    public override void OnLeftRoom()
    {
        requestPending = false;

        if (readyButton != null)
        {
            readyButton.interactable = false;
        }

        if (readyButtonText != null)
        {
            readyButtonText.text = readyLabel;
        }
    }


}
