using System;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/*
 * Gestisce graficamente una singola Room nella lista Multiplayer
 * 
 * Riceve i dati di una Room Photon e aggiorna:
 * nome della Room, numero di giocatori e pulsante per entrare nella Room
 */

public class RoomListItemUI : MonoBehaviour
{

    [Header("Texts")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text joinButtonText;

    [Header("Button")]
    [SerializeField] private Button joinButton;

    private string roomName;
    private bool roomIsJoinable;
    private Action<string> joinRequested;

    private void Awake()
    {
        if (joinButton == null)
        {
            Debug.LogError("[RoomListItemUI] JoinButton non assegnato.", this);

            return;
        }

        joinButton.onClick.AddListener(HandleJoinButtonClicked);
    }


    // Inizializza la riga usando i dati ricevuti da Photon
    public void Initialize(RoomInfo roomInfo, Action<string> onJoinRequested)
    {
        if (roomInfo == null)
        {
            Debug.LogError("[RoomListItemUI] RoomInfo non valido.", this);

            return;
        }

        roomName = roomInfo.Name;
        joinRequested = onJoinRequested;

        if (roomNameText != null)
        {
            roomNameText.text = roomInfo.Name;
        }

        string maximumPlayersText = roomInfo.MaxPlayers == 0 ? "∞" : roomInfo.MaxPlayers.ToString();

        if (playerCountText != null)
        {
            playerCountText.text = $"{roomInfo.PlayerCount} / {maximumPlayersText}";
        }

        bool hasFreeSlot = roomInfo.MaxPlayers == 0 || roomInfo.PlayerCount < roomInfo.MaxPlayers;

        roomIsJoinable = roomInfo.IsOpen && hasFreeSlot;

        if (joinButtonText != null)
        {
            joinButtonText.text = roomIsJoinable ? "JOIN" : "FULL";
        }

        SetInteractionEnabled(true);
    }


    // Abilita / disabilita temporaneamente l'interazione con il pulsante di Join
    public void SetInteractionEnabled(bool interactionEnabled)
    {
        if (joinButton == null)
        {
            return;
        }

        joinButton.interactable = interactionEnabled && roomIsJoinable;
    }

    private void HandleJoinButtonClicked()
    {
        if (!roomIsJoinable)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(roomName))
        {
            return;
        }

        joinRequested?.Invoke(roomName);
    }

    private void OnDestroy()
    {
        if (joinButton != null)
        {
            joinButton.onClick.RemoveListener(HandleJoinButtonClicked);
        }
    }


}
