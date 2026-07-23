using System.Collections;
using UnityEngine;
using UnityEngine.UI;


/*
 * Spostamento orizzontale di un Content di una Lista UI utilizzando esclusivamente le due frecce
 * 
 * 
 */



public sealed class HorizontalArrowScroller : MonoBehaviour
{

    [Header("Scroll References")]
    [SerializeField] private RectTransform viewport;
    [SerializeField] private RectTransform content;

    [Header("Arrow Buttons")]
    [SerializeField] private Button leftArrowButton;
    [SerializeField] private Button rightArrowButton;

    [Header("Movement")]
    [SerializeField] [Min(0.01f)] private float animationDuration = 0.15f;
    [SerializeField] [Min(1f)] private float fallbackStep = 200f;
    [SerializeField] private bool disableButtonsAtLimits = true;

    private HorizontalLayoutGroup horizontalLayoutGroup;
    private Coroutine movementCoroutine;


    private void Awake()
    {
        if (content != null)
        {
            horizontalLayoutGroup = content.GetComponent<HorizontalLayoutGroup>();
        }
    }


   
    private void OnEnable()
    {
        if (leftArrowButton != null)
        {
            leftArrowButton.onClick.AddListener(ScrollLeft);
        }

        if (rightArrowButton != null)
        {
            rightArrowButton.onClick.AddListener(ScrollRight);
        }

        StartCoroutine(RefreshOnNextFrame());
    }

    // Il Content viene spostato verso destra, mostrando gli elementi alla sua sinistra
    public void ScrollLeft()
    {
        MoveContent(GetScrollStep());
    }

    // Il Content viene spostato verso sinistra, mostrando gli elementi alla sua destra
    public void ScrollRight()
    {
        MoveContent(-GetScrollStep());
    }

    // Aspetta un frame prima di aggiornare il layout, utile per assicurarsi che le dimensioni del Content siano corrette
    private IEnumerator RefreshOnNextFrame()
    {
        yield return null;

        RefreshLayout();
    }


    // Aggiorna il layout del Content e delle frecce
    private void MoveContent(float horizontalAmount)
    {
        if (viewport == null || content == null)
        {
            Debug.LogError("[HorizontalArrowScroller] " + "Viewport o Content non assegnati.", this);

            return;
        }

        RefreshLayout();

        float minimumX = GetMinimumContentX();

        float targetX = Mathf.Clamp(content.anchoredPosition.x + horizontalAmount, minimumX, 0f);

        if (Mathf.Approximately(targetX, content.anchoredPosition.x))
        {
            RefreshArrowButtons();
            return;
        }

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        movementCoroutine = StartCoroutine(AnimateMovement(targetX));
    }




    // Anima il Content dalla posizione attuale fino alla posizione target.
    private IEnumerator AnimateMovement(float targetX)
    {
        Vector2 startPosition = content.anchoredPosition;

        Vector2 targetPosition = new Vector2(targetX, startPosition.y);

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;

            float normalizedTime = Mathf.Clamp01(elapsedTime / animationDuration);

            float smoothTime = normalizedTime * normalizedTime * (3f - 2f * normalizedTime);

            content.anchoredPosition = Vector2.Lerp(
                startPosition,
                targetPosition,
                smoothTime
            );

            yield return null;
        }

        content.anchoredPosition = targetPosition;

        movementCoroutine = null;

        RefreshArrowButtons();
    }


    // Calcola di quanto deve muoversi la lista ad ogni pressione della freccia. 
    // Lo spostamento corrisponde alla larghezza del primo elemento visibile più lo spacing tra gli elementi, se presente.
    private float GetScrollStep()
    {
        if (content == null)
        {
            return fallbackStep;
        }

        for (int index = 0; index < content.childCount; index++)
        {
            RectTransform child = content.GetChild(index) as RectTransform;

            if (child == null || !child.gameObject.activeSelf)
            {
                continue;
            }

            float spacing = horizontalLayoutGroup != null ? horizontalLayoutGroup.spacing : 0f;

            return child.rect.width + spacing;
        }

        return fallbackStep;
    }



    // Calcola il limite sinistro massimo del Content
    private float GetMinimumContentX()
    {
        float scrollableWidth = Mathf.Max(0f, content.rect.width - viewport.rect.width);

        return -scrollableWidth;
    }


    // Ricalcola immediatamente il layout del Content e aggiorna lo stato delle frecce in base alla posizione attuale del Content
    private void RefreshLayout()
    {
        if (viewport == null || content == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        Vector2 currentPosition = content.anchoredPosition;

        currentPosition.x = Mathf.Clamp( currentPosition.x, GetMinimumContentX(), 0f);

        content.anchoredPosition = currentPosition;

        RefreshArrowButtons();
    }

    // Aggiorna lo stato delle frecce in base alla posizione attuale del Content
    private void RefreshArrowButtons()
    {
        if (viewport == null || content == null)
        {
            return;
        }

        if (!disableButtonsAtLimits)
        {
            if (leftArrowButton != null)
            {
                leftArrowButton.interactable = true;
            }

            if (rightArrowButton != null)
            {
                rightArrowButton.interactable = true;
            }

            return;
        }


        const float tolerance = 0.5f;

        float currentX = content.anchoredPosition.x;

        float minimumX = GetMinimumContentX();

        if (leftArrowButton != null)
        {
            leftArrowButton.interactable = currentX < -tolerance;
        }

        if (rightArrowButton != null)
        {
            rightArrowButton.interactable = currentX > minimumX + tolerance;
        }
    }


}
