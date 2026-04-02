using System.Collections;
using UnityEngine;

public class UIPanelAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform background;

    [Header("Underlying Panel")]
    [SerializeField] private GameObject underlyingPanel;

    [Header("Animation")]
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private Vector3 closedScale = new Vector3(0.85f, 0.85f, 1f);
    [SerializeField] private Vector3 openScale = Vector3.one;
    [SerializeField] private Vector3 overshootScale = new Vector3(1.05f, 1.05f, 1f);

    private Coroutine currentRoutine;

    public void Open()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        gameObject.SetActive(true);
        currentRoutine = StartCoroutine(OpenRoutine());
    }

    public void Close()
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(CloseRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        float time = 0f;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        background.localScale = closedScale;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(time / duration);
            float ease = 1f - Mathf.Pow(1f - progress, 3f);

            canvasGroup.alpha = ease;

            Vector3 targetScale;
            if (progress < 0.8f)
            {
                float t1 = progress / 0.8f;
                targetScale = Vector3.Lerp(closedScale, overshootScale, t1);
            }
            else
            {
                float t2 = (progress - 0.8f) / 0.2f;
                targetScale = Vector3.Lerp(overshootScale, openScale, t2);
            }

            background.localScale = targetScale;

            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        background.localScale = openScale;

        if (underlyingPanel != null)
            underlyingPanel.SetActive(false);

        currentRoutine = null;
    }

    private IEnumerator CloseRoutine()
    {
        float time = 0f;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 1f;

        Vector3 startScale = background.localScale;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(time / duration);
            float ease = 1f - Mathf.Pow(1f - progress, 3f);

            background.localScale = Vector3.Lerp(startScale, closedScale, ease);

            yield return null;
        }

        background.localScale = closedScale;
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);

        if (underlyingPanel != null) underlyingPanel.SetActive(true);

        currentRoutine = null;
    }
}