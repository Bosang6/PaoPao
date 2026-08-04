using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public sealed class DarknessMaskController : MonoBehaviour
{
    private Camera worldCamera;
    private Transform target;

    private float lightRadius = 1f;
    private float edgeSoftness = 1f;
    private Color darknessColor = new Color(0f, 0f, 0f, 0.99f);

    private RawImage rawImage;
    private Material runtimeMaterial;

    private static readonly int CenterId =
        Shader.PropertyToID("_Center");

    private static readonly int RadiusId =
        Shader.PropertyToID("_Radius");

    private static readonly int SoftnessId =
        Shader.PropertyToID("_Softness");

    private static readonly int DarkColorId =
        Shader.PropertyToID("_DarkColor");


    private void Awake()
    {
        rawImage = GetComponent<RawImage>();

        if (rawImage.material == null)
        {
            Debug.LogError(
                "DarknessOverlay: Not found mask material.",
                this
            );

            enabled = false;
            return;
        }

        // Create a Runtime Material instance to dynamically modify shader parameters.
        runtimeMaterial = Instantiate(rawImage.material);
        runtimeMaterial.name = rawImage.material.name + "_Runtime";

        rawImage.material = runtimeMaterial;
        rawImage.raycastTarget = false;
    }


    public void Initialize(
        Camera cameraReference,
        Transform targetReference,
        float radius,
        float softness,
        Color darkColor)
    {
        worldCamera = cameraReference;
        target = targetReference;

        lightRadius = Mathf.Max(radius, 0.01f);
        edgeSoftness = Mathf.Max(softness, 0f);
        darknessColor = darkColor;

        UpdateMask();
    }


    private void LateUpdate()
    {
        UpdateMask();
    }


    private void UpdateMask()
    {
        if (worldCamera == null ||
            target == null ||
            runtimeMaterial == null)
        {
            return;
        }

        // The player's position on the screen space, ranges from 0 to 1.
        Vector3 centerViewport = worldCamera.WorldToViewportPoint(target.position);

        // Convert the world space radius to the screen space radius.
        Vector3 radiusWorldPosition = target.position + worldCamera.transform.up * lightRadius;
        Vector3 radiusViewport = worldCamera.WorldToViewportPoint(radiusWorldPosition);
        float viewportRadius = Mathf.Abs(radiusViewport.y - centerViewport.y);

        // Calculate the width of the gradient region on the screen.
        Vector3 outerWorldPosition = target.position + worldCamera.transform.up * (lightRadius + edgeSoftness);
        Vector3 outerViewport = worldCamera.WorldToViewportPoint(outerWorldPosition);
        float outerRadius = Mathf.Abs(outerViewport.y - centerViewport.y);
        float viewportSoftness = Mathf.Max(outerRadius - viewportRadius, 0.0001f);

        // Set material params
        runtimeMaterial.SetVector(CenterId, new Vector4(centerViewport.x, centerViewport.y, 0f, 0f));
        runtimeMaterial.SetFloat(RadiusId, viewportRadius);
        runtimeMaterial.SetFloat(SoftnessId, viewportSoftness);
        runtimeMaterial.SetColor(DarkColorId, darknessColor);
    }


    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}