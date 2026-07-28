using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class UIGameManager : MonoBehaviourPunCallbacks
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject quitDialogPanel;
    [SerializeField] private CanvasGroup mobileControlsCanvasGroup;

    [Header("Final Timer Text")]
    [SerializeField] private TextMeshProUGUI winTimeText;
    [SerializeField] private TextMeshProUGUI loseTimeText;

    [Header("Final Kills Text")]
    [SerializeField] private TextMeshProUGUI winKillText;
    [SerializeField] private TextMeshProUGUI loseKillText;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    // Impedisce di inviare più richieste di uscita mentre Photon sta lasciando la Room
    private bool isLeavingRoom;



    private void Start()
    {
        HideAllPanels();
        SetMobileControlsVisible(true);
        Time.timeScale = 1f;
        isPaused = false;
    }


    // UTILITY
    private void HideAllPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (quitDialogPanel != null) quitDialogPanel.SetActive(false);
    }

    private void SetMobileControlsVisible(bool visible)
    {
        if (mobileControlsCanvasGroup == null)
            return;

        mobileControlsCanvasGroup.alpha = visible ? 1f : 0f;
        mobileControlsCanvasGroup.interactable = visible;
        mobileControlsCanvasGroup.blocksRaycasts = visible;
    }



    // PAUSE
    // Single Player: ferma il gioco tramite Time.timeScale
    // Multiplayer: apre soltatno il menu locale, gli altri player continuano a giocare
    public void OnPausePressed()
    {
        if (isPaused || isLeavingRoom) return;

        isPaused = true;

        if (!PhotonNetwork.InRoom) Time.timeScale = 0f;

        SetMobileControlsVisible(false);
        pausePanel.SetActive(true);
    }

    // RESUME
    // SinglePlayer: viene riattivato il timer
    // Multiplayer: continua 
    public void OnResumePressed()
    {
        if (!isPaused) return;

        isPaused = false;

        if (!PhotonNetwork.InRoom) Time.timeScale = 1f;

        SetMobileControlsVisible(true);
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

    // Single Player: carica direttamente il menu
    // Multiplayer: lascia la Room di Photon poi carica il menu
    public void OnConfirmQuitPressed()
    {
        ReturnToMainMenu();
    }


    // WIN / LOSE

    // Mostra la vittoria nella modalità single player.
    public void ShowWinPanel(string finalTime, string localKillCounter)
    {
        if (winPanel == null)
        {
            Debug.LogWarning("[UIGameManager] WinPanel non presente nella scena.", this);
            return;
        }

        HideAllPanels();
        SetMobileControlsVisible(false);
        Time.timeScale = 0f;
        isPaused = false;
        if (winTimeText != null) winTimeText.text = finalTime;
        if (winKillText != null) winKillText.text = localKillCounter;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayWinSound();
        winPanel.SetActive(true);
    }

    // Mostra la sconfitta nella modalità single player.
    public void ShowLosePanel(string finalTime, string localKillCounter)
    {
        if (losePanel == null)
        {
            Debug.LogWarning("[UIGameManager] LosePanel non presente nella scena.", this);
            return;
        }

        HideAllPanels();
        SetMobileControlsVisible(false);
        Time.timeScale = 0f;
        isPaused = false;
        if (loseTimeText != null) loseTimeText.text = finalTime;
        if (loseKillText != null) loseKillText.text = localKillCounter;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayLoseSound();
        losePanel.SetActive(true);
    }


    // Single Player: ricarica la partita
    // Multiplayer: se è il Master, ricarica la partita per tutti. Se client invia la richiesta
    public void OnReplayPressed()
    {
        Time.timeScale = 1f;

        if (PhotonNetwork.InRoom)
        {
            if (PhotonGameManager.Instance == null)
            {
                Debug.LogError("[UIGameManager] PhotonGameManager non presente nella scena.", this);
                return;
            }

            PhotonGameManager.Instance.RequestReplay();
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }


    public void OnMainMenuPressed()
    {
        ReturnToMainMenu();
    }



    // MULTIPLAYER

    // Avvia il ritorno al Menu
    // Se il client si trova in una Room, invia una richiesta di uscita, aspetta OnLeftRoom,
    // altrimenti carica direttamente il Main Menu 
    private void ReturnToMainMenu()
    {
        if (isLeavingRoom)
        {
            return;
        }

        Time.timeScale = 1f;

        isPaused = false;

        SetMobileControlsVisible(false);

        HideAllPanels();

        if (!PhotonNetwork.InRoom)
        {
            LoadMainMenu();
            return;
        }

        isLeavingRoom = true;

        // false indica che il player deve lasciare definitivamente la Room e non restare inattivo
        bool requestStarted = PhotonNetwork.LeaveRoom(false);

        if (!requestStarted)
        {
            Debug.LogError("[UIGameManager] " + "Photon non ha accettato la richiesta LeaveRoom.", this);
            isLeavingRoom = false;
            LoadMainMenu();
        }
    }


    // Photon richiama questo metodo quando il client ha completato l'uscita dalla Room
    public override void OnLeftRoom()
    {
        if (!isLeavingRoom) return;

        isLeavingRoom = false;
        LoadMainMenu();
    }


    // In PaoPao non supportiamo la migrazione del Master Client
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (isLeavingRoom || !PhotonNetwork.InRoom)
        {
            return;
        }

        Debug.LogWarning("[UIGameManager] Il Master Client ha lasciato la Room. La sessione viene terminata per tutti.", this);
        ReturnToMainMenu();
    }


    // Carica localmente il Main Menu quando il client non appartiene più alla partita Multiplayer
    private void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

}




