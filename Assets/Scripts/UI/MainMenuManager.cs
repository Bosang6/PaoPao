using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager: MonoBehaviour
{

    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    public void OnPlayPressed()
    {
        
    }


    public void OnSettingPressed()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }


    public void OnQuitPressed()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }



    public void OnBackPressed()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }





}
