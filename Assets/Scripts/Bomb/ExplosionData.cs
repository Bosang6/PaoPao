using UnityEngine;

[CreateAssetMenu(fileName = "ExplosionData", menuName = "PaoPaoOBJ/ExplosionData")]
public class ExplosionData : ScriptableObject
{
    [Header("Explosion Settings")]
    public float fTimeToExplode = 3f;      //Tempo prima che esploda la bomba
    public int iRange = 1;                  //Distanza coperta dall'esplosione della bomba
    public float fFlameDuration = 0.5f;     //Durata fiamme dovute all'esplosione 
    public int iDamage = 1;                 //Danno inflitto dalla bomba


    [Header("Layers")]
    public LayerMask lmWallLayer;           //Muri indistruttibili
    public LayerMask lmBreakableLayer;      //Muri distruttibili
    public LayerMask lmPlayerLayer;         //Player
    
}
