using UnityEngine;

public class PauseMenuManager: MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private UITimer uiTimer;

    private bool isPaused = false;

    private void Start()
    {
        if(pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        Time.timeScale = 1f; // Assicurati che il gioco parta in modalità non pausa
        isPaused = false;
    }

    public void OpenPauseMenu()
    {
        if (isPaused) return;

        isPaused = true;

        if(pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if(uiTimer != null)  uiTimer.StopTimer();

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if(!isPaused) return;

        isPaused = false;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (uiTimer != null) uiTimer.StartTimer();

        Time.timeScale = 1f;
    }


}
