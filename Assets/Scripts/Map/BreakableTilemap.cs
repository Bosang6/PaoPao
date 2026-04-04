using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableTilemap : MonoBehaviour
{
    private Tilemap tilemap;

    [SerializeField]
    private GameObject breakEffectPrefab;

    [SerializeField]
    private string targetTag = "Flame";
    
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!other.CompareTag(targetTag)) return;

        // La fiamma è già centrata sulla cella, usiamo direttamente la sua posizione
        Vector3Int cellPos = tilemap.WorldToCell(other.transform.position);
        TileBase tile = tilemap.GetTile(cellPos);

        if (tile == null) return;

        Vector3 effectPos = tilemap.GetCellCenterWorld(cellPos);

        if (breakEffectPrefab != null)
            Instantiate(breakEffectPrefab, effectPos, Quaternion.identity);

        tilemap.SetTile(cellPos, null);
    }
    /*
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag(targetTag)) return;
        
        // Loop through all collision contact points
        for (int i = 0; i < other.contactCount; i++)
        {
            ContactPoint2D contact = other.GetContact(i);
            
            // Move the contact point slightly inside the tile to avoid touching adjacent tiles.
            Vector2 hitPoint = contact.point + contact.normal * 0.1f;
            Vector3Int cellPos = tilemap.WorldToCell(hitPoint);
            TileBase tile = tilemap.GetTile(cellPos);
            
            if(tile == null) continue;
            
            // Get the world-space center position of the collided tile.
            Vector3 effectPos = tilemap.GetCellCenterWorld(cellPos);
            
            // Instantiate the Bricks Break Effect at the obtained position.
            if (breakEffectPrefab != null)
            {
                Instantiate(breakEffectPrefab, effectPos, Quaternion.identity);
            }
            tilemap.SetTile(cellPos, null);
            
        }
    }
    */
    
}
