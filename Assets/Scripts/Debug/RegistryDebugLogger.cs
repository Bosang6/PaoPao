using UnityEngine;

//[AI] SCRIPT TEMPORANEO per il test di M1. Da attaccare a un GameObject qualsiasi della scena.
//Stampa nel Console le bombe e le fiamme attive: posizione e tempo residuo.
//Verrà sostituito dai Gizmos (M3/M9) e si potrà eliminare.
public class RegistryDebugLogger : MonoBehaviour
{
    [SerializeField] private float logInterval = 0.5f;  //ogni quanti secondi stampare
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < logInterval) return;
        timer = 0f;

        if (BombPool.Instance == null || FlamePool.Instance == null) return;

        foreach (BombController bomb in BombPool.Instance.ActiveBombs)
        {
            Debug.Log($"[M1] Bomba in {bomb.Position}: esplode tra {bomb.TimeToExplode:F2} s");
        }

        foreach (FlameController flame in FlamePool.Instance.ActiveFlames)
        {
            Debug.Log($"[M1] Fiamma in {flame.Position}: si spegne tra {flame.RemainingTime:F2} s");
        }
    }
}