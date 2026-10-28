using System.Collections.Generic;
using UnityEngine;

public class BombPool : MonoBehaviour
{
    public static BombPool Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private int initialPoolSize = 7;   //Pool di bombe

    private Queue<BombController> pool = new Queue<BombController>();

    //[AI] Registro delle bombe attualmente in gioco (cioè quelle fuori dal pool).
    //Il bot lo legge per sapere dove sono le bombe e quando esplodono.
    private readonly List<BombController> activeBombs = new List<BombController>();
    public IReadOnlyList<BombController> ActiveBombs => activeBombs;

    private void Awake()
    {
        //Singleton, ne posso avere solo uno per esecuzione
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        //Pre-istanzia le bombe, anziché farle tutte dopo
        for(int i = 0; i < initialPoolSize; i++) { pool.Enqueue(CreateBomb()); }
    }

    //[AI] Crea una bomba spenta. NON la mette nella coda: lo decide chi la chiama.
    //(Prima la accodava sempre: se il pool era vuoto, Get() riceveva una bomba che
    //era ANCHE in coda, e al Get() successivo la stessa bomba veniva riutilizzata
    //mentre era ancora in gioco.)
    BombController CreateBomb()
    {
        GameObject obj = Instantiate(bombPrefab, transform);
        BombController bomb = obj.GetComponent<BombController>();
        obj.SetActive(false);
        return bomb;
    }


    public BombController Get(Vector3 position)
    {
        BombController bomb = pool.Count > 0 ? pool.Dequeue() : CreateBomb();   //crea solo se pool vuoto
        bomb.transform.position = position; //Assegna alla bomba l'attuale posizione del player
        bomb.gameObject.SetActive(true);
        activeBombs.Add(bomb);              //[AI] registra la bomba come attiva
        return bomb;
    }

    public void ReturnToPool(BombController bomb)
    {
        activeBombs.Remove(bomb);           //[AI] non è più attiva
        bomb.gameObject.SetActive(false);
        pool.Enqueue(bomb);
    }

    //[AI] C'è già una bomba attiva su questa cella? (cella = posizione allineata alla griglia)
    public bool IsBombAt(Vector3 cell)
    {
        for (int i = 0; i < activeBombs.Count; i++)
        {
            if (Vector3.Distance(activeBombs[i].Position, cell) < 0.1f) { return true; }
        }
        return false;
    }

}