using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFitMap : MonoBehaviour
{
    [Header("Map Size")]
    [SerializeField] private float mapWidth = 16f;
    [SerializeField] private float mapHeight = 13f;
    [SerializeField] private float padding = 0.5f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        FitCameraToMap();
    }

    /* Questa funzione adatta la dim ortografica della camera in modo da far rientrare tutta la mappa, 
     * cosi da essere sicuri che i giocatori vedano tutto il campo di gioco, indipendentemente dalle dimensioni dello schermo.
     */
    private void FitCameraToMap()
    {
        float screenAspect = (float)Screen.width / Screen.height;
        float mapAspect = mapWidth / mapHeight;

        if(screenAspect >= mapAspect)
        {
            cam.orthographicSize = (mapHeight * 0.5f) + padding;
        }
        else
        {
            float sizeBasedOnWidth = (mapWidth / screenAspect) * 0.5f;
            cam.orthographicSize = sizeBasedOnWidth + padding;
        }

        transform.position = new Vector3(
            0f,
            0f,
            transform.position.z
        );

    }

}
