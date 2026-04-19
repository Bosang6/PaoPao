using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UITitleMenuManager : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float delayBeforeLoad = 0.2f;

    private bool isLoading = false;

    public void OnStartGamePressed()
    {
        if (isLoading)
            return;

        StartCoroutine(LoadMainMenuRoutine());
    }

    private IEnumerator LoadMainMenuRoutine()
    {
        isLoading = true;

        yield return new WaitForSecondsRealtime(delayBeforeLoad);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}