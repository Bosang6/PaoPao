using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private readonly List<Transform> players = new List<Transform>();
    private readonly Dictionary<Vector2Int, Transform> reservedCells = new Dictionary<Vector2Int, Transform>();
    private readonly Dictionary<Transform, Vector2Int> reservedByPlayer = new Dictionary<Transform, Vector2Int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Register(Transform player)
    {
        if (player != null && !players.Contains(player)) players.Add(player);
    }

    public void Unregister(Transform player)
    {
        players.Remove(player);
        ReleaseMove(player);
    }

    public List<Transform> GetAllPlayers() => players;

    public bool IsCellReservedByOther(Transform player, Vector3 position, float cellSize)
    {
        if (cellSize <= 0f) return false;
        Vector2Int cell = new Vector2Int(
            Mathf.RoundToInt(position.x / cellSize),
            Mathf.RoundToInt(position.y / cellSize));
        return reservedCells.TryGetValue(cell, out Transform owner) && owner != null && owner != player;
    }
    public bool TryReserveMove(Transform player, Vector3 destination, float cellSize)
    {
        if (player == null || cellSize <= 0f) return false;
        Vector2Int cell = new Vector2Int(
            Mathf.RoundToInt(destination.x / cellSize),
            Mathf.RoundToInt(destination.y / cellSize));

        if (reservedCells.TryGetValue(cell, out Transform owner) && owner != null && owner != player)
            return false;

        ReleaseMove(player);
        reservedCells[cell] = player;
        reservedByPlayer[player] = cell;
        return true;
    }

    public void ReleaseMove(Transform player)
    {
        if (player == null || !reservedByPlayer.TryGetValue(player, out Vector2Int cell)) return;
        if (reservedCells.TryGetValue(cell, out Transform owner) && owner == player)
            reservedCells.Remove(cell);
        reservedByPlayer.Remove(player);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
