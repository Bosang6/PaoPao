using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    /* Manager che gestisce il Main Menu
     Attiva/disattiva i panel e dal MatchSetupPanel indirizza il giocatore alla mappa
     Inotre attiva i flag per le animazioni
    */

    [Header("Scenes")]
    [SerializeField] private string GameMap1 = "GameMap_1" ;
    [SerializeField] private string GameMap2 = "GameMap_2";

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject matchSetupPanel;

    [Header("Selection Glow")]
    [SerializeField] private GameObject selectionGlow_1;
    [SerializeField] private GameObject selectionGlow_2;


    private int selectedMapIndex = -1;

    private void Start()
    {
        // Check dei panel e attiva il main menu
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        if(settingsPanel != null) settingsPanel.SetActive(false);

        if(matchSetupPanel != null) matchSetupPanel.SetActive(false);
    }


    /* Main Menu */

    public void OnPlayMenuPressed()
    {
        // Attiva il match panel
        mainMenuPanel.SetActive(false);
        matchSetupPanel.SetActive(true);
    }

    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
    }

    /* Settings Menu */
    public void OnSettingPressed()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnSettingsBackPressed()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }


    /* Match Setup Menu */

    public void SelectMap(int mapIndex)
    {
        selectedMapIndex = mapIndex;
        
        selectionGlow_1.SetActive(mapIndex == 0);
        selectionGlow_2.SetActive(mapIndex == 1);

        Debug.Log("Mappa selezionata: " + selectedMapIndex);

    }

    public void OnConfirmPlayPressed()
    {
        if (selectedMapIndex == -1) Debug.Log("Mappa non selezionata");

        if(selectedMapIndex == 0) SceneManager.LoadScene(GameMap1);
        else if(selectedMapIndex == 1) SceneManager.LoadScene(GameMap2);
    }

    public void OnMatchSetupBackPressed()
    {
        mainMenuPanel.SetActive(true);
        matchSetupPanel.SetActive(false);
    }














}
