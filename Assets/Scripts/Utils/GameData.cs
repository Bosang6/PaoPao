using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "PaoPaoOBJ/GameData")]
public class GameData : ScriptableObject
{
    [Header("Game Settings")]
    public int timer = 500; //Da impostare
    public int nMappa = 1;  //Da impostare

    [Header("Grid Settings")]
    public float fCellSize = 1f;            //Dimensione della tile
}
