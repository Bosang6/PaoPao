using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform target;
    [SerializeField] private float pressedScale = 0.9f;
    [SerializeField] private float speed = 12f;

    private Vector3 defaultScale;

    private void Awake()
    {
        if (target == null)
            target = transform as RectTransform;

        defaultScale = target.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        target.localScale = defaultScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        target.localScale = defaultScale;
    }

    private void OnDisable()
    {
        target.localScale = defaultScale;
    }
}