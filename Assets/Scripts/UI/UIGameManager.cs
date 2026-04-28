using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIGameManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject quitDialogPanel;
    [SerializeField] private GameObject mobileControlsPanel;

    [Header("Final Timer Text")]
    [SerializeField] private TextMeshProUGUI winTimeText;
    [SerializeField] private TextMeshProUGUI loseTimeText;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;


    private void Start()
    {
        HideAllPanels();
        SetMobileControlsActive(true);
        Time.timeScale = 1f;
        isPaused = false;
    }


    // UTILITY
    private void HideAllPanels()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        quitDialogPanel.SetActive(false);
    }

    private void SetMobileControlsActive(bool active)
    {
        if (mobileControlsPanel != null) mobileControlsPanel.SetActive(active);
    }



    // PAUSE
    public void OnPausePressed()
    {
        if(isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;

        SetMobileControlsActive(false);
        pausePanel.SetActive(true);
    }

    public void OnResumePressed()
    {
        isPaused = false;
        Time.timeScale = 1f;

        SetMobileControlsActive(true);
        pausePanel.SetActive(false);
    }

    public void OnSettingsPressed()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnQuitGamePressed()
    {
        pausePanel.SetActive(false);
        quitDialogPanel.SetActive(true);
    }


    // SETTINGS
    public void OnSettingsBackPressed()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

  
    // Aggiungere bottone Apply 


    // QUIT GAME
    public void OnCancelQuitPressed()
    {
        quitDialogPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void OnConfirmQuitPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }


    // WIN / LOSE

    public void ShowWinPanel(string finalTime)
    {
        HideAllPanels();
        Time.timeScale = 0f;
        isPaused = false;
        if (winTimeText != null) winTimeText.text = finalTime;
        if(AudioManager.Instance != null) AudioManager.Instance.PlayWinSound();
        winPanel.SetActive(true);
    }

    public void ShowLosePanel(string finalTime)
    {
        HideAllPanels();
        Time.timeScale = 0f;
        isPaused = false;
        if (loseTimeText != null) loseTimeText.text = finalTime;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayLoseSound();
        losePanel.SetActive(true);
    }

    public void OnReplayPressed()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void OnMainMenuPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }


}
