using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "PaoPaoOBJ/GameData")]
public class GameData : ScriptableObject
{
    [Header("Game Settings")]
    public int timer = 500; //Da impostare
    public int nMappa = 1;  //Da impostare

    [Header("Tile layers")]
    public LayerMask lmIcePlate;           //Tile per lastre di ghiaccio

    [Header("Grid Settings")]
    public float fCellSize = 1f;            //Dimensione della tile

    [Header("Player collision")]
    public LayerMask lmCollisionLayer;      //Layer con i quali i player collidono
    //04/05/2026: Wall, Player, Breakable, Bomb
}
