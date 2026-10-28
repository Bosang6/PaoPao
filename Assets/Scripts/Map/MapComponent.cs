using UnityEngine;

public class MapComponent : MonoBehaviour
{
    [SerializeField] private Vector2Int[] playerPos = null;

    //[AI] Limiti dell'arena: celle GIOCABILI (dentro il bordo di muri), estremi inclusi.
    //Con fCellSize = 1 le coordinate di cella coincidono con quelle del mondo.
    //I valori di default sono solo le celle di spawn: vanno verificati per ogni mappa.
    [Header("Limiti arena (celle giocabili, estremi inclusi)")]
    [SerializeField] private Vector2Int arenaMin = new Vector2Int(-6, -5);
    [SerializeField] private Vector2Int arenaMax = new Vector2Int(6, 5);

    public Vector2Int ArenaMin => arenaMin;
    public Vector2Int ArenaMax => arenaMax;
    public int Width => arenaMax.x - arenaMin.x + 1;
    public int Height => arenaMax.y - arenaMin.y + 1;

    public Vector2Int[] GetPlayerSpawnPos()
    {
        return playerPos;
    }

    //[AI] Disegna il rettangolo dell'arena nella Scene quando selezioni la mappa:
    //serve per controllare a colpo d'occhio che i limiti siano giusti.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = new Vector3((arenaMin.x + arenaMax.x) / 2f, (arenaMin.y + arenaMax.y) / 2f, 0f);
        Vector3 size = new Vector3(Width, Height, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}