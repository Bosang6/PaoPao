using UnityEngine;


/*
 * Adatta il RectTransform alla Safe Area del dispositivo.
 *
 * Viene eseguito solamente durante il Play Mode o nella build,
 * evitando di modificare e corrompere prefab e scene nell'Editor.
 */


[RequireComponent(typeof(RectTransform))]
public sealed class UISafeAreaFitter : MonoBehaviour
{
    private RectTransform rectTransform;

    private Rect lastSafeArea;

    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;


    // Recupera il RectTransfomr e applica la Safe Area quando il pannello viene attivato
    private void OnEnable()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }


    // Aggiorna Layout soltanto quando cambiano: safe area - larghezza schermo - altezza schermo
    private void Update()
    {
        if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight && Screen.safeArea == lastSafeArea)
        {
            return;
        }

        ApplySafeArea();
    }


    // Converte la Safe Area da pixel ad Ancher normalizzati
    private void ApplySafeArea()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

     
        // Evita divisioni per zero durante le inizializzaioni, cambi di scena o condizioni tempornaemente non valide
        if (screenWidth <= 0 || screenHeight <= 0)
        {
            return;
        }

        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = new Vector2(safeArea.xMin / screenWidth, safeArea.yMin / screenHeight);

        Vector2 anchorMax = new Vector2(safeArea.xMax / screenWidth, safeArea.yMax / screenHeight);

        // Se da problemi di NaN
        if (!IsFinite(anchorMin.x) || !IsFinite(anchorMin.y) || !IsFinite(anchorMax.x) || !IsFinite(anchorMax.y))
        {
            Debug.LogWarning("[UISafeAreaFitter] " + "Impossibile applicare la Safe Area: valori non validi.",this);
            return;
        }

        anchorMin.x = Mathf.Clamp01(anchorMin.x);
        anchorMin.y = Mathf.Clamp01(anchorMin.y);

        anchorMax.x = Mathf.Clamp01(anchorMax.x);
        anchorMax.y = Mathf.Clamp01(anchorMax.y);

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        lastSafeArea = safeArea;
        lastScreenWidth = screenWidth;
        lastScreenHeight = screenHeight;
    }


    // Restituisce true solamente per valori numerici validi
    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}