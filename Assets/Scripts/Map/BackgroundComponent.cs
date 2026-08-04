using UnityEngine;

public class BackgroundComponent : MonoBehaviour
{
    void Start()
    {
        Fit();
    }
    
    private void Fit()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (!sr.sprite)
        {
            return;
        }
        
        Camera cam = Camera.main;

        float cameraHeight = cam.orthographicSize * 2f;
        float cameraWidth = cameraHeight * cam.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size;

        float scaleX = cameraWidth / spriteSize.x;
        float scaleY = cameraHeight / spriteSize.y;

        float scale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
