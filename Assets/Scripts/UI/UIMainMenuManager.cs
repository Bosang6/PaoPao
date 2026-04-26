using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMainMenuManager : MonoBehaviour
{

    /* Manager che gestisce il Main Menu
     Attiva/disattiva i panel e dal MatchSetupPanel indirizza il giocatore nella partita con la mappa selezionata
     Inotre attiva i flag per le animazioni
    */

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject matchSetupPanel;

    [Header("Selection Glow")]
    [SerializeField] private GameObject springGlow;
    [SerializeField] private GameObject winterGlow;

    [Header("Buttons")]
    [SerializeField] private Button matchPlayButton;

    [Header("Timing")]
    [SerializeField] private float buttonDelay = 0.2f;


    private E_Map? selectedMap = null;
    private bool isBusy = false;

    private void Start()
    {
        // Check dei panel e attiva il main menu
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if(settingsPanel != null) settingsPanel.SetActive(false);
        if(matchSetupPanel != null) matchSetupPanel.SetActive(false);

        // MenuMusic
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMusic();

        ResetMapSelection();
    }



    /* Main Menu */
    public void OnPlayMenuPressed()
    {
        // Attiva il match panel
        if(isBusy) return;
        StartCoroutine(OpenMatchSetupRoutine());
    }

    public void OnSettingsPressed()
    {
        if (isBusy) return;
        StartCoroutine(OpenSettingsRoutine());
    }

    public void OnQuitPressed()
    {
        if (isBusy) return;
        StartCoroutine(QuitRoutine());
    }



    /* Settings Menu */
    public void OnSettingsBackPressed()
    {
        if (isBusy) return;
        StartCoroutine(BackFromSettingsRoutine());
    }



    /* Match Setup Menu */
    public void SelectMap(E_Map map)
    {
        selectedMap = map;

        if(springGlow != null) springGlow.SetActive(map == E_Map.Spring);
        if(winterGlow != null) winterGlow.SetActive(map == E_Map.Winter);

        UpdateMatchPlayButton();

        Debug.Log("Mappa selezionata: " + map);
    }

    public void OnConfirmPlayPressed()
    {
        if (isBusy) return;
        StartCoroutine(ConfirmPlayRoutine());
    }

    public void OnMatchSetupBackPressed()
    {
        if (isBusy) return;
        StartCoroutine(BackFromMatchSetupRoutine());
    }



    /* Utility */
    private IEnumerator OpenMatchSetupRoutine()
    {
        isBusy = true;
        yield return new WaitForSecondsRealtime(buttonDelay);
        mainMenuPanel.SetActive(false);
        matchSetupPanel.SetActive(true);
        isBusy = false;
    }

    private IEnumerator BackFromMatchSetupRoutine()
    {
        isBusy = true;
        yield return new WaitForSecondsRealtime(buttonDelay);
        if (matchSetupPanel != null) matchSetupPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        ResetMapSelection();
        isBusy = false;
    }

    private IEnumerator OpenSettingsRoutine()
    {
        isBusy = true;
        yield return new WaitForSecondsRealtime(buttonDelay);
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        isBusy = false;
    }

    private IEnumerator BackFromSettingsRoutine()
    {
        isBusy = true;
        yield return new WaitForSecondsRealtime(buttonDelay);
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        isBusy = false;
    }

    private IEnumerator ConfirmPlayRoutine()
    {
        isBusy = true;

        if (selectedMap == null)
        {
            Debug.LogWarning("Nessuna mappa selezionata.");
            isBusy = false;
            yield break;
        }

        yield return new WaitForSecondsRealtime(buttonDelay);

        GameSession.SelectedMap = selectedMap.Value;

        //STOP MUSIC
        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic();

        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator QuitRoutine()
    {
        isBusy = true;

        yield return new WaitForSecondsRealtime(buttonDelay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }


    /* Helpers */

    private void ResetMapSelection()
    {
        selectedMap = null;
        if (springGlow != null) springGlow.SetActive(false);
        if (winterGlow != null) winterGlow.SetActive(false);

        UpdateMatchPlayButton();
    }

    private void UpdateMatchPlayButton()
    {
        if (matchPlayButton != null)
            matchPlayButton.interactable = (selectedMap != null);
    }
}
