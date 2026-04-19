using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIWLAnimation : MonoBehaviour
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
        // Salva le posizioni target finali 
        if (leftItem != null) leftTargetPosition = leftItem.rectTransform.anchoredPosition;
        if (rightItem != null) rightTargetPosition = rightItem.rectTransform.anchoredPosition;
    }


    private void OnEnable()
    {
        PlayAnimation();
    }


    public void PlayAnimation()
    {

        // Avvia la coroutine 

        if (leftItem == null || rightItem == null)
        {
            Debug.LogWarning($"{name}: assegna leftItem e rightItem nell'Inspector.");
            return;
        }

        if (currentRoutine != null) StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        /* Mette gli elementi fuori posizione 
         * Resetta la rotazione
         * Applica un delay
         * Animazione di entrata
         * Li rimette perfetti alla fine
        */

        RectTransform leftRect = leftItem.rectTransform;
        RectTransform rightRect = rightItem.rectTransform;

        Vector2 leftStart = leftTargetPosition + Vector2.left * horizontalOffset;
        Vector2 rightStart = rightTargetPosition + Vector2.right * horizontalOffset;

        leftRect.anchoredPosition = leftStart;
        rightRect.anchoredPosition = rightStart;

        leftRect.localRotation = Quaternion.identity;
        rightRect.localRotation = Quaternion.identity;

        yield return new WaitForSecondsRealtime(delayBeforeStart);

        float time = 0f;

        while (time < animationDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / animationDuration);
            float eased = EaseOutCubic(t);

            leftRect.anchoredPosition = Vector2.Lerp(leftStart, leftTargetPosition, eased);
            rightRect.anchoredPosition = Vector2.Lerp(rightStart, rightTargetPosition, eased);

            leftRect.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 360, eased));
            rightRect.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, -360, eased));

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
        // Trasforma un movimento lineare in uno più naturale
        return 1f - Mathf.Pow(1f - t, 3f);
    }
}