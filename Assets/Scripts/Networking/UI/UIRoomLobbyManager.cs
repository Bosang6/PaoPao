using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;


/*
 * Gestisce delle informazioni principali mostrate nella parte superiore del RoomLobbyPanel
 * 
 * Responsabilità:
 * - mostrare il nome della Room corrente
 * - mostrare il numero dei giocatori presenti
 * - aggiornare il contatore quando un player entra o esce
 * - pulire i testi quando il client lascia la Room
 * 
 */

public sealed class UIRoomLobbyManager : MonoBehaviourPunCallbacks
{
    [Header("Room Information")]
    [Tooltip("Testo che mostra il nome della Room corrente.")]
    [SerializeField] private TMP_Text roomNameText;
    [Tooltip("Testo che mostra il numero di giocatori presenti nella Room.")]
    [SerializeField] private TMP_Text playerCountText;


    // Viene eseguito ogni volta che il RoomLobbyPanel viene attivato
    // Leggiamo immediatamente i dati della Room perchè il pannello può essere attivato dopo che OnJoinedRoom
    // è stato chiamato
    public override void OnEnable()
    {
        base.OnEnable();

        RefreshRoomInformation();
    }

    // Disattiva correttamente la registrazione delle callback Photon gestita dalla classe base
    public override void OnDisable()
    {
        base.OnDisable();
    }


    // Viene chiamato sui client già presenti quando un nuovo player entra nella Room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshRoomInformation();
    }

    // Viene chiamato sui client già presenti quando un player lascia la Room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshRoomInformation();
    }


    // Viene chiamato quando il client localte ha completato l'uscita della Room
    public override void OnLeftRoom()
    {
        ClearRoomInformation();
    }



    // Legge i dati della Room corrente ed aggiorna i due testi della UI
    private void RefreshRoomInformation()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            ClearRoomInformation();
            return;
        }

        Room currentRoom = PhotonNetwork.CurrentRoom;

        if (roomNameText != null)
        {
            roomNameText.text = currentRoom.Name;
        }

        if (playerCountText != null)
        {
            playerCountText.text = $"PLAYERS: {currentRoom.PlayerCount} / " + $"{currentRoom.MaxPlayers}";
        }
    }


    // Ripristina i testi quando non siamo all'interno di una Room
    private void ClearRoomInformation()
    {
        if (roomNameText != null)
        {
            roomNameText.text = "ROOM NAME";
        }

        if (playerCountText != null)
        {
            playerCountText.text = $"PLAYERS: 0 / {PhotonRoomSlotManager.SlotCount}";
        }
    }




}
