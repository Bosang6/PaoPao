using UnityEngine;

public static class GridUtils
{
    public static Vector3 AdjustPosition(Vector3 pos, float fCellSize)
    {
        /*  A causa delle moltiplicazioni tra float, a lungo andare 
         *  la posizione potrebbe essere leggermente traslata, questa 
         *  funzione permette di aggiustare la posizione, rimettendola 
         *  in linea con la griglia */

        float newX = Mathf.Round(pos.x / fCellSize) * fCellSize;
        float newY = Mathf.Round(pos.y / fCellSize) * fCellSize;
        return new Vector3(newX, newY, pos.z);  //La z rimane invariata
    }
}
