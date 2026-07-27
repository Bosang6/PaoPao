using TMPro;
using UnityEngine;
using UnityEngine.UI;


/*
 * Gestisce la schermata mostrata alla fine di una partita multiplayer
 * 
 * Responsabilità:
 * - mostrare il risultato locale WIN oppure LOSE
 * - visualizzare tempo e uccisioni
 * - inizializzare le righe dei partecipanti
 * - aggiornare lo stato Ready dei singoli slot
 * - controllare graficamente i pulsanti post-partita
 * 
 */

[DisallowMultipleComponent]
public sealed class MultiplayerResultPanelController : MonoBehaviour
{
    [Header("Result")]
    [SerializeField] private TextMeshProUGUI resultTitleText;

    [Header("Match Stats")]
    [SerializeField] private TextMeshProUGUI timeValueText;
    [SerializeField] private TextMeshProUGUI killsValueText;

    [Header("Player Rows")]
    [Tooltip("Righe ordinate per SlotIndex: 0, 1, 2, 3.")]
    [SerializeField] private MultiplayerResultPlayerRow[] playerRows;

    [Header("Buttons")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private TextMeshProUGUI playAgainButtonText;

    [SerializeField] private Button returnMenuButton;

    [Header("Decorations")]
    [SerializeField] private GameObject star1;
    [SerializeField] private GameObject star2;



    // Mostra il pannello e inserisce i dati relativi alla partita appena terminata
    public void ShowResult(bool localPlayerWon, string finalTime, string localKillCount)
    {
        gameObject.SetActive(true);

        if (resultTitleText != null)
        {
            resultTitleText.text = localPlayerWon ? "WIN" : "LOSE";
        }

        if (timeValueText != null)
        {
            timeValueText.text = string.IsNullOrWhiteSpace(finalTime) ? "00:00" : finalTime;
        }

        if (killsValueText != null)
        {
            killsValueText.text = string.IsNullOrWhiteSpace(localKillCount) ? "0" : localKillCount;
        }

        InitializePlayerRows();

        // Le animazioni delle stelle verranno aggiunte dopo.
        SetStarsVisible(false);

        SetPlayAgainButtonState("PLAY AGAIN", true);

        if (returnMenuButton != null)
        {
            returnMenuButton.interactable = true;
        }
    }



    private void InitializePlayerRows()
    {
        HideAllRows();

        if (GameSession.PlayerSlots == null || GameSession.PlayerSlots.Count == 0)
        {
            Debug.LogWarning("[MultiplayerResultPanel] " + "Nessuna configurazione presente in GameSession.", this);
            return;
        }

        foreach (PlayerSlotConfig slot in GameSession.PlayerSlots)
        {
            if (slot == null)
            {
                continue;
            }

            int slotIndex = slot.slotIndex;

            if (!IsValidRowIndex(slotIndex))
            {
                Debug.LogWarning("[MultiplayerResultPanel] " + $"Riga non disponibile per lo Slot {slotIndex}.", this);
                continue;
            }

            MultiplayerResultPlayerRow row = playerRows[slotIndex];

            if (row == null)
            {
                continue;
            }

            string playerLabel = $"PLAYER {slotIndex + 1}";

            E_RematchPlayerStatus initialStatus = slot.pType == PlayerInstanceData.E_PlayerSlotType.AI ? E_RematchPlayerStatus.AI : E_RematchPlayerStatus.NotReady;

            row.Configure(slotIndex, playerLabel, initialStatus);
        }
    }


    // Aggiorna lo stato di una singola riga.
    // Verrà richiamato Photon quando comunicherà: ready - not ready - left - ai
    public void SetPlayerStatus(int slotIndex, E_RematchPlayerStatus status)
    {
        if (!IsValidRowIndex(slotIndex))
        {
            return;
        }

        MultiplayerResultPlayerRow row = playerRows[slotIndex];

        if (row == null)
        {
            return;
        }

        row.SetStatus(status);
    }




    // Cambia testo e interattività del pulsante principale
    public void SetPlayAgainButtonState(string label, bool interactable)
    {
        if (playAgainButtonText != null)
        {
            playAgainButtonText.text = label;
        }

        if (playAgainButton != null)
        {
            playAgainButton.interactable = interactable;
        }
    }


    // Mostra o nasconde le due descrizioni
    public void SetStarsVisible(bool visible)
    {
        if (star1 != null)
        {
            star1.SetActive(visible);
        }

        if (star2 != null)
        {
            star2.SetActive(visible);
        }
    }


    // Nascondo completamente la schermata
    public void HidePanel()
    {
        gameObject.SetActive(false);
    }


    private void HideAllRows()
    {
        if (playerRows == null)
        {
            return;
        }

        foreach (MultiplayerResultPlayerRow row in playerRows)
        {
            row?.Hide();
        }
    }


    private bool IsValidRowIndex(int slotIndex)
    {
        return playerRows != null &&
               slotIndex >= 0 &&
               slotIndex < playerRows.Length;
    }


}
