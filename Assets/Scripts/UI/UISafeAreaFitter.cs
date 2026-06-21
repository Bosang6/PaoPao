using UnityEngine;


/*
 * Script che adatta automaticamente questo RectTransform alla SafeArea del dispositivo.
 * La SafeArea evita notch, brodi arrotondati e barre di sistema.
 * 
 */

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UISafeAreaFitter : MonoBehaviour
{

    private RectTransform rectTransform;
    private Rect lastSafeArea;

    // Recupera il RectTransform quando lo script viene caricato
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }


    private void Update()
    {
        if(lastSafeArea != Screen.safeArea)
        {
            ApplySafeArea();
        }
    }

    // Converte la Safe Area da pixel ad anchor e la applica al RectTransform
    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

    }
}
