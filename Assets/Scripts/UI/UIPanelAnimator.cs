using System.Collections;
using UnityEngine;

public class UIPanelAnimator : MonoBehaviour
{
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform background;
    [SerializeField] private float duration = 0.3f;

    private void OnEnable()
    {
        StartCoroutine(OpenRoutine());
    }

    public void Close()
    {
        StartCoroutine(CloseRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        float t = 0f;

        panelGroup.alpha = 0f;
        background.localScale = new Vector3(0.9f, 0.9f, 1f);

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / duration;

            panelGroup.alpha = progress;
            background.localScale = Vector3.Lerp(new Vector3(0.9f, 0.9f, 1f), Vector3.one, progress);

            yield return null;
        }

        panelGroup.alpha = 1f;
        background.localScale = Vector3.one;
    }

    private IEnumerator CloseRoutine()
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / duration;

            panelGroup.alpha = 1f - progress;
            background.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.9f, 0.9f, 1f), progress);

            yield return null;
        }

        gameObject.SetActive(false);
    }
}