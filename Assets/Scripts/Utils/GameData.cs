using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "PaoPaoOBJ/GameData")]
public class GameData : ScriptableObject
{
    [Header("Grid Settings")]
    public float fCellSize = 1f;            //Dimensione della tile
}
