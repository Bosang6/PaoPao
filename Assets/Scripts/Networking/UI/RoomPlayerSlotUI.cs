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

    [Header("Status Sprites")]
    [Tooltip("Indicatore verde mostrato quando il player è pronto.")]
    [SerializeField] private Sprite readySprite;
    [Tooltip("Indicatore arancione mostrato quando il player non è pronto.")]
    [SerializeField] private Sprite notReadySprite;
    [Tooltip("Indicatore blu utilizzato per gli slot controllati dalla AI.")]
    [SerializeField] private Sprite aiSprite;

    [Header("Ready Labels")]
    [SerializeField] private string readyLabel = "READY";
    [SerializeField] private string notReadyLabel = "NOT READY";
    [SerializeField] private string emptyLabel = "WAITING";

    private Image readyIndicatorImage;

    [SerializeField] private string aiLabel = "AI";



    // Recupera il componente Image presente sull'indicatore Ready
    private void Awake()
    {
        if (readyIndicator != null)
        {
            readyIndicatorImage = readyIndicator.GetComponent<Image>();
        }
    }


    // Mostra lo slot come occupato
    public void ShowOccupied(bool isLocalPlayer, bool isMasterClient, Sprite characterSprite, bool isReady)
    {
        if (characterImage != null)
        {
            characterImage.sprite = characterSprite;

            characterImage.gameObject.SetActive(characterSprite != null);
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
            readyIndicator.SetActive(true);
        }

        // Indicator image
        if (readyIndicatorImage != null)
        {
            readyIndicatorImage.sprite = isReady ? readySprite : notReadySprite;
            readyIndicatorImage.color = Color.white;
        }

        // Status
        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);

            statusText.text = isReady ? readyLabel : notReadyLabel;
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


    // Mostra lo slot come controllato dalla AI
    public void ShowAI(Sprite characterSprite)
    {
        if (characterImage != null)
        {
            characterImage.sprite = characterSprite;
            characterImage.gameObject.SetActive(characterSprite != null);
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
            readyIndicator.SetActive(true);
        }

        if (readyIndicatorImage != null)
        {
            readyIndicatorImage.sprite = aiSprite;
            readyIndicatorImage.color = Color.white;
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = aiLabel;
        }
    }


}
