using UnityEngine;
using System.Collections;

public sealed class CaveLightingInstaller : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField]
    private GameObject darknessCanvasPrefab;

    [Header("Player")]
    [Tooltip("Player Layer")]
    [SerializeField]
    private LayerMask playerLayer;

    [Header("Light Settings")]
    [Min(0.01f)]
    [SerializeField]
    private float lightRadius = 1f;

    [Min(0f)]
    [SerializeField]
    private float edgeSoftness = 1f;

    [SerializeField]
    private Color darknessColor = new Color(0f, 0f, 0f, 0.99f);

    private GameObject darknessCanvasInstance;

    private IEnumerator Start()
    {
        if (darknessCanvasPrefab == null)
        {
            Debug.LogError(
                "CaveLightingInstaller：Not found DarknessCanvasPrefab.",
                this
            );

            yield break;
        }

        Camera worldCamera = null;
        Transform player = null;
        
        // Get camera and player reference
        while (worldCamera == null || player == null)
        {
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }

            if (player == null)
            {
                player = FindLocalPlayer();
            }

            yield return null;
        }

        darknessCanvasInstance = Instantiate(darknessCanvasPrefab);

        DarknessMaskController controller = darknessCanvasInstance.GetComponentInChildren<DarknessMaskController>(true);

        if (controller == null)
        {
            Debug.LogError(
                "DarknessCanvasPrefab: Not found DarknessMaskController.",
                darknessCanvasInstance
            );

            Destroy(darknessCanvasInstance);
            yield break;
        }

        controller.Initialize(worldCamera, player, lightRadius, edgeSoftness, darknessColor);
    }
    
    private Transform FindLocalPlayer()
    {
        int playerLayer = LayerMask.NameToLayer("Player");

        if (playerLayer == -1)
        {
            Debug.LogError(
                "Not found Player Layer。",
                this
            );

            return null;
        }

        PlayerController[] playerControllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        // Looking for local player
        foreach (PlayerController playerController in playerControllers)
        {
            bool isOnPlayerLayer = playerController.gameObject.layer == playerLayer;
            bool isLocalHumanPlayer = playerController.IsHuman;

            if (isOnPlayerLayer && isLocalHumanPlayer)
            {
                return playerController.transform;
            }
        }

        return null;
    }


    private void OnDestroy()
    {
        if (darknessCanvasInstance != null)
        {
            Destroy(darknessCanvasInstance);
        }
    }
}