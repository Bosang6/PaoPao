using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenuManager : MonoBehaviour
{
    [SerializeField] private string mainMenu;
    [SerializeField] private Button startButton;

    private bool isLoading = false;

    public void OnStartGamePressed()
    {
        if (isLoading)
            return;

        StartCoroutine(StartGameRoutine());
    }


    private IEnumerator StartGameRoutine()
    {
        isLoading = true;

        if (startButton != null) startButton.interactable = false;

        float delay = 0f;

        // Play suono
        if (AudioManager.Instance != null)
            delay = AudioManager.Instance.PlayButtonClick();

        // Aspetta fine suono
        yield return new WaitForSecondsRealtime(delay);

        SceneManager.LoadScene(mainMenu);
    }
}