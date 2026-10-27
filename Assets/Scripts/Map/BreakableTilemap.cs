using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableTilemap : MonoBehaviour
{
    public static BreakableTilemap Instance { get; private set; }
    private static readonly List<BreakableTilemap> Instances = new List<BreakableTilemap>();
    private Tilemap tilemap;
    private Tilemap wallTilemap;

    [SerializeField]
    private GameObject breakEffectPrefab;

    [SerializeField]
    private string targetTag = "Flame";
    
    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        Instance = this;
        if (!Instances.Contains(this)) Instances.Add(this);
        Tilemap[] tilemaps = transform.parent != null
            ? transform.parent.GetComponentsInChildren<Tilemap>()
            : GetComponentsInChildren<Tilemap>();
        foreach (Tilemap candidate in tilemaps)
            if (candidate != null && candidate.gameObject.layer == LayerMask.NameToLayer("Wall"))
                wallTilemap = candidate;
    }

    public static bool HasBreakableAtWorldPosition(Vector3 worldPosition)
    {
        foreach (BreakableTilemap instance in Instances)
            if (instance != null && instance.tilemap != null &&
                instance.tilemap.HasTile(instance.tilemap.WorldToCell(worldPosition))) return true;
        return false;
    }

    public static bool HasWallAtWorldPosition(Vector3 worldPosition)
    {
        foreach (BreakableTilemap instance in Instances)
            if (instance != null && instance.wallTilemap != null &&
                instance.wallTilemap.HasTile(instance.wallTilemap.WorldToCell(worldPosition))) return true;
        return false;
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
        if (Instance == this) Instance = Instances.Count > 0 ? Instances[Instances.Count - 1] : null;
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!other.CompareTag(targetTag)) return;

        // La fiamma � gi� centrata sulla cella, usiamo direttamente la sua posizione
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
