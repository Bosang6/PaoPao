using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "PaoPaoOBJ/GameData")]
public class GameData : ScriptableObject
{
    [Header("Tile layers")]
    public LayerMask lmIcePlate;           //Tile per lastre di ghiaccio

    [Header("Grid Settings")]
    public float fCellSize = 1f;            //Dimensione della tile

    public int localPlayerKill = 0;

    [Header("Player collision")]
    public LayerMask lmCollisionLayer;      //Layer con i quali i player collidono
    //04/05/2026: Wall, Player, Breakable, Bomb
}
