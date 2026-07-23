using TMPro;
using UnityEngine;
using UnityEngine.UI;


/*
 * Gestisce graficamente un singolo player slot presente nel RoomLobbyPanel
 * 
 * 
 */


public sealed class RoomPlayerSlotUI : MonoBehaviour
{
    [Header("Player References")]
    [Tooltip("Glow mostrato solamente sullo slot del giocatore locale.")]
    [SerializeField] private GameObject localPlayerGlow;
    [Tooltip("Icona mostrata solamente sullo slot del Master Client.")]
    [SerializeField] private GameObject hostIcon;
    [Tooltip("Immagine temporanea del personaggio nello slot.")]
    [SerializeField] private Image characterImage;

    [Header("Status References")]
    [Tooltip("Indicatore Ready. Per ora resta nascosto.")]
    [SerializeField]private GameObject readyIndicator;
    [Tooltip("Testo che descrive lo stato temporaneo dello slot.")]
    [SerializeField] private TMP_Text statusText;

    [Header("Temporary Labels")]
    [SerializeField] private string occupiedLabel = "CONNECTED";
    [SerializeField] private string emptyLabel = "WAITING";


    // Mostra lo slot come occupato
    // is LocalPlayer: indica se il giocatore locale è quello nello slot
    // is MasterClient: indica se il giocatore nello slot è il Master Client
    public void ShowOccupied(bool isLocalPlayer, bool isMasterClient)
    {
        if (characterImage != null)
        {
            characterImage.gameObject.SetActive(true);
        }

        if (localPlayerGlow != null)
        {
            localPlayerGlow.SetActive(isLocalPlayer);
        }

        if (hostIcon != null)
        {
            hostIcon.SetActive(isMasterClient);
        }

        if (readyIndicator != null)
        {
            readyIndicator.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = occupiedLabel;
        }
    }


    // Mostra lo stato come libero 
    public void ShowEmpty()
    {
        if (characterImage != null)
        {
            characterImage.gameObject.SetActive(false);
        }

        if (localPlayerGlow != null)
        {
            localPlayerGlow.SetActive(false);
        }

        if (hostIcon != null)
        {
            hostIcon.SetActive(false);
        }

        if (readyIndicator != null)
        {
            readyIndicator.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = emptyLabel;
        }
    }


}
