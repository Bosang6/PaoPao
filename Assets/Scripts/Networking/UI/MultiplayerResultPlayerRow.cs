using TMPro;
using UnityEngine;
using UnityEngine.UI;



// Stati che un player può assumere nella schermata post-partita
public enum E_RematchPlayerStatus
{
    NotReady,
    Ready,
    Left,
    AI
}


/*
 * Gestisce graficamente una singola riga della tabella presente a fine partita
 * 
 * Responsabilità:
 * - mostrare il nome o il numero del giocatore
 * - mostrare l'icona dello stato
 * - mostrare READY, NOT READY, LEFT oppure AI
 * - nascondere la riga quando lo slot non viene utilizzato
 */


[DisallowMultipleComponent]
public sealed class MultiplayerResultPlayerRow : MonoBehaviour
{

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI playerText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Status Icon")]
    [SerializeField] private Image statusIcon;

    [Header("Status Sprites")]
    [SerializeField] private Sprite readySprite;
    [SerializeField] private Sprite notReadySprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite aiSprite;


    public int SlotIndex{get; private set;}


    // Configura completamente la riga
    // Verrà richiamato quando il pannello viene aperto o quando cambia lo stato di un giocatore
    public void Configure(int slotIndex, string playerLabel, E_RematchPlayerStatus playerStatus)
    {
        SlotIndex = slotIndex;

        gameObject.SetActive(true);

        if (playerText != null)
        {
            playerText.text = playerLabel;
        }

        SetStatus(playerStatus);
    }


    // Aggiorna solamente lo stato del giocatore senza modificarne il suo nome
    public void SetStatus(E_RematchPlayerStatus playerStatus)
    {
        switch (playerStatus)
        {
            case E_RematchPlayerStatus.Ready:
                ApplyStatus("READY", readySprite);
                break;

            case E_RematchPlayerStatus.NotReady:
                ApplyStatus("NOT READY", notReadySprite);
                break;

            case E_RematchPlayerStatus.Left:
                ApplyStatus("LEFT", leftSprite);
                break;

            case E_RematchPlayerStatus.AI:
                ApplyStatus("AI", aiSprite);
                break;

            default:
                ApplyStatus("NOT READY", notReadySprite);
                break;
        }
    }


    // Applica testo e simbolo dello stato alla riga
    private void ApplyStatus(string label, Sprite icon)
    {
        if (statusText != null)
        {
            statusText.text = label;
        }

        if (statusIcon != null)
        {
            statusIcon.sprite = icon;
            statusIcon.enabled = icon != null;
        }
    }

    // Nasconde la riga quando lo slot non deve essere visualizzato
    public void Hide()
    {
        gameObject.SetActive(false);
    }





}

