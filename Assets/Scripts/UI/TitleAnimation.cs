using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TitleAnimation : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private Image leftItem;
    [SerializeField] private Image rightItem;

    [Header("Timing")]
    [SerializeField] private float delayBeforeStart = 2f;
    [SerializeField] private float animationDuration = 4f;

    [Header("Start Offset")]
    [SerializeField] private float horizontalOffset = 500f;

    private Vector2 leftTargetPosition;
    private Vector2 rightTargetPosition;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (leftItem != null)
            leftTargetPosition = leftItem.rectTransform.anchoredPosition;

        if (rightItem != null)
            rightTargetPosition = rightItem.rectTransform.anchoredPosition;
    }

    public void PlayAnimation()
    {
        if (leftItem == null || rightItem == null)
        {
            Debug.LogWarning($"{name}: assegna leftItem e rightItem nell'Inspector.");
            return;
        }

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        RectTransform leftRect = leftItem.rectTransform;
        RectTransform rightRect = rightItem.rectTransform;

        Vector2 leftStartPosition = leftTargetPosition + Vector2.left * horizontalOffset;
        Vector2 rightStartPosition = rightTargetPosition + Vector2.right * horizontalOffset;

        leftRect.anchoredPosition = leftStartPosition;
        rightRect.anchoredPosition = rightStartPosition;

        leftRect.localRotation = Quaternion.identity;
        rightRect.localRotation = Quaternion.identity;

        yield return new WaitForSecondsRealtime(delayBeforeStart);

        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / animationDuration);

            float easedT = EaseOutCubic(t);

            leftRect.anchoredPosition = Vector2.Lerp(leftStartPosition, leftTargetPosition, easedT);
            rightRect.anchoredPosition = Vector2.Lerp(rightStartPosition, rightTargetPosition, easedT);

            float leftRotation = Mathf.Lerp(0f, 360f, easedT);
            float rightRotation = Mathf.Lerp(0f, -360f, easedT);

            leftRect.localRotation = Quaternion.Euler(0f, 0f, leftRotation);
            rightRect.localRotation = Quaternion.Euler(0f, 0f, rightRotation);

            yield return null;
        }

        leftRect.anchoredPosition = leftTargetPosition;
        rightRect.anchoredPosition = rightTargetPosition;

        leftRect.localRotation = Quaternion.identity;
        rightRect.localRotation = Quaternion.identity;

        currentRoutine = null;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}