using UnityEngine;
using UnityEngine.SceneManagement;

public class TestManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private UIPanelAnimator settingsPanel;
    [SerializeField] private UIPanelAnimator quitConfirmPanel;
    [SerializeField] private string mainMenu;



    public void OnResumePressed()
    {
        pauseMenuPanel.SetActive(false);
    }

    public void OnBackSettingsPressed()
    {
        settingsPanel.Close();
    }


    public void OnCancelPressed()
    {
        quitConfirmPanel.Close();
    }

    public void OnConfirmQuitPressed()
    {
        SceneManager.LoadScene(mainMenu);
    }
}