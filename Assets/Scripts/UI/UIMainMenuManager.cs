using System.Collections;
using System.Collections.Generic;
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

    [Header("Map Selection Glow")]
    [SerializeField] private GameObject springGlow;
    [SerializeField] private GameObject winterGlow;

    [Header("Match Preset")]
    [SerializeField] private SinglePlayerMatchPreset singlePlayerMatchPreset;

    [Header("Player Selecion Glow")]
    [SerializeField] private GameObject bombermanGlow;
    [SerializeField] private GameObject penguinGlow;


    [Header("Buttons")]
    [SerializeField] private Button matchPlayButton;
    [SerializeField] private Button multiPlayButton;

    [Header("Timing")]
    [SerializeField] private float buttonDelay = 0.2f;


    private E_Map? selectedMap = null;
    private CharacterData.E_Character? selectedCharacter = null;
    private bool isBusy = false;

    private void Start()
    {
        // Check dei panel e attiva il main menu
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if(settingsPanel != null) settingsPanel.SetActive(false);
        if(matchSetupPanel != null) matchSetupPanel.SetActive(false);

        //Per il tasto Multiplayer (da togliere in futuro)
        if (multiPlayButton != null) multiPlayButton.interactable = false;

        // MenuMusic
        if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMusic();

        ResetMapSelection();
        ResetCharacterSelection();
    }



    /* Main Menu */
    public void OnSinglePlayMenuPressed()
    {
        // Attiva il match panel
        if(isBusy) return;
        StartCoroutine(OpenMatchSetupRoutine());
    }

    public void OnMultiPlayMenuPressed()
    {
        if(isBusy) return;
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

        //Debug.Log("Mappa selezionata: " + map);
    }

    public void SelectCharacter(CharacterData.E_Character cType)
    {
        selectedCharacter = cType;

        if (bombermanGlow != null) bombermanGlow.SetActive(cType == CharacterData.E_Character.Bomberman);
        if (penguinGlow != null) penguinGlow.SetActive(cType == CharacterData.E_Character.Penguin);

        UpdateMatchPlayButton();

        //Debug.Log("Character selezionato : " + character);
    }


    public void SelectSpringMap()
    {
        SelectMap(E_Map.Spring);
    }

    public void SelectWinterMap()
    {
        SelectMap(E_Map.Winter);
    }

    public void SelectBombermanCharacter()
    {
        //Debug.Log("Bomeberman selzionato");
        SelectCharacter(CharacterData.E_Character.Bomberman);
    }

    public void SelectPenguinCharacter()
    {
        Debug.Log("Penguin selzionato");
        SelectCharacter(CharacterData.E_Character.Penguin);
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
        ResetCharacterSelection();
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

        if(selectedCharacter == null)
        {
            Debug.LogWarning("Nessun character selezionato.");
            isBusy = false;
            yield break;
        }

        yield return new WaitForSecondsRealtime(buttonDelay);

        // Configura la partita con la mappa ed il character selezionato
        if (singlePlayerMatchPreset == null)
        {
            Debug.LogWarning("SinglePlayerMatchPreset non assegnato.");
            isBusy = false;
            yield break;
        }

        List<PlayerSlotConfig> slots = singlePlayerMatchPreset.BuildSlots(selectedCharacter.Value);

        GameSession.SetMatchConfig(selectedMap.Value, slots);


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

    private void ResetCharacterSelection()
    {
        selectedCharacter = null;
        if(bombermanGlow != null) bombermanGlow.SetActive(false);
        if(penguinGlow != null) penguinGlow.SetActive(false);

        UpdateMatchPlayButton();
    }

    private void UpdateMatchPlayButton()
    {
        if (matchPlayButton != null)
            matchPlayButton.interactable = (selectedMap != null && selectedCharacter != null);
    }
}
