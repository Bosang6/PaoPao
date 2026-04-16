using UnityEngine;

public class MapComponent : MonoBehaviour
{
    [SerializeField] private Vector2Int[] playerPos = null;

    public Vector2Int[] GetPlayerSpawnPos()
    {
        return playerPos;
    }
}
