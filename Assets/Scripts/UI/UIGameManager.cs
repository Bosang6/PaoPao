using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


/*
 * Gestisce i pannelli principali dell'interfaccia durante la partita
 *
 * Nel Single Player controlla anche pausa, risultato e Replay.
 * Nel Multiplayer il menu di pausa è solamente locale e l'uscita dalla partita viene gestita attraverso la Room Photon.
 */

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

    private bool isPaused;
    private bool isLeavingRoom;


    // Ripristina lo stato iniziale dell'interfaccia e del tempo di gioco
    private void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;

        HideAllPanels();
        SetMobileControlsVisible(true);
    }


    // Nasconde tutti i pannelli gestiti da questo componente
    private void HideAllPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (quitDialogPanel != null) quitDialogPanel.SetActive(false);
    }


    // Mostra o nasconde i controlli mobile e ne aggiorna l'interazione.
    private void SetMobileControlsVisible(bool visible)
    {
        if (mobileControlsCanvasGroup == null) return;
   
        mobileControlsCanvasGroup.alpha = visible ? 1f : 0f;
        mobileControlsCanvasGroup.interactable = visible;
        mobileControlsCanvasGroup.blocksRaycasts = visible;
    }


    /*
     * Apre il menu di pausa.
     *
     * Nel Single Player ferma il tempo.
     * Nel Multiplayer mette in pausa solamente l'interfaccia locale.
     */
    // Apre il menu di pausa
    // Single Player: ferma il tempo
    // Multiplayer: mette in pausa l'interfaccia locale
    public void OnPausePressed()
    {
        if (isPaused || isLeavingRoom) return;

        isPaused = true;

        if (!PhotonNetwork.InRoom) Time.timeScale = 0f;

        SetMobileControlsVisible(false);

        if (pausePanel != null) pausePanel.SetActive(true);
    }


    // Chiude il menu di pausa e ripristina i controlli del giocatore.
    public void OnResumePressed()
    {
        if (!isPaused) return;
  
        isPaused = false;

        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 1f;
        }

        SetMobileControlsVisible(true);

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }


    // Apre il pannello delle impostazioni dal menu di pausa
    public void OnSettingsPressed()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }


    // Torna al menu di pausa dal pannello delle impostazioni.
    public void OnSettingsBackPressed()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }


    // Apre la finestra di conferma per abbandonare la partita.
    public void OnQuitGamePressed()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (quitDialogPanel != null)
        {
            quitDialogPanel.SetActive(true);
        }
    }


    // Chiude la finestra di conferma e torna al menu di pausa.
    public void OnCancelQuitPressed()
    {
        if (quitDialogPanel != null)
        {
            quitDialogPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }


    // Conferma l'uscita dalla partita e avvia il ritorno al menu
    public void OnConfirmQuitPressed()
    {
        ReturnToMainMenu();
    }


    // Mostra il pannello di vittoria nella modalità Single Player
    public void ShowWinPanel(string finalTime, string localKillCounter)
    {
        ShowSinglePlayerResult(
            winPanel,
            winTimeText,
            winKillText,
            finalTime,
            localKillCounter,
            true
        );
    }


    // Mostra il pannello di sconfitta nella modalità Single Player.
    public void ShowLosePanel(string finalTime, string localKillCounter)
    {
        ShowSinglePlayerResult(
            losePanel,
            loseTimeText,
            loseKillText,
            finalTime,
            localKillCounter,
            false
        );
    }


    // Configura e mostra il pannello finale del Single Player.
    private void ShowSinglePlayerResult(
        GameObject resultPanel,
        TextMeshProUGUI timeText,
        TextMeshProUGUI killText,
        string finalTime,
        string localKillCounter,
        bool isWin
    )
    {
        if (resultPanel == null)
        {
            Debug.LogWarning("[UIGameManager] Pannello del risultato non presente nella scena.", this);
            return;
        }

        HideAllPanels();
        SetMobileControlsVisible(false);

        Time.timeScale = 0f;
        isPaused = false;

        if (timeText != null)
        {
            timeText.text = finalTime;
        }

        if (killText != null)
        {
            killText.text = localKillCounter;
        }

        if (AudioManager.Instance != null)
        {
            if (isWin)
            {
                AudioManager.Instance.PlayWinSound();
            }
            else
            {
                AudioManager.Instance.PlayLoseSound();
            }
        }

        resultPanel.SetActive(true);
    }


    // SinglePlayer: ricarica la scena corrente
    // Multiplayer: viene gestita separatamente dal PhotonRematchManager
    public void OnReplayPressed()
    {
        Time.timeScale = 1f;

        Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }


    // Avvia il ritorno al Main Menu dal pannello finale.
    public void OnMainMenuPressed()
    {
        ReturnToMainMenu();
    }


    // SinglePlayer: carica sunbito la scena
    // Multiplayer: lascia la Room di Photon
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

        bool requestStarted = PhotonNetwork.LeaveRoom(false);

        if (requestStarted)
        {
            return;
        }

        Debug.LogError("[UIGameManager] Photon non ha accettato la richiesta LeaveRoom.", this);

        isLeavingRoom = false;
        LoadMainMenu();
    }


    // Carica il Main Menu dopo aver completato l'uscita dalla Room.
    public override void OnLeftRoom()
    {
        if (!isLeavingRoom)
        {
            return;
        }

        isLeavingRoom = false;

        LoadMainMenu();
    }


    // Termina la sessione quando l'Host lascia la Room
    public override void OnMasterClientSwitched(
        Player newMasterClient
    )
    {
        if (isLeavingRoom || !PhotonNetwork.InRoom)
        {
            return;
        }

        Debug.LogWarning("[UIGameManager] Il Master Client ha lasciato la Room. La sessione viene terminata.", this);
        ReturnToMainMenu();
    }


    // Carica localmente la scena del Main Menu.
    private void LoadMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}