using System.Collections.Generic;
using UnityEngine;

public class BombPool : MonoBehaviour
{
    public static BombPool Instance { get; private set; }

    [Header("Pool Settings")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private int initialPoolSize = 7;   //Pool di bombe

    private Queue<BombController> pool = new Queue<BombController>();

    private void Awake()
    {
        //Singleton, ne posso avere solo uno per esecuzione
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        //Pre-istanzia le bombe, anzichè farle tutte dopo
        for(int i = 0; i < initialPoolSize; i++) { CreateBomb(); }
    }

    BombController CreateBomb()
    {
        GameObject obj = Instantiate(bombPrefab, transform);
        BombController bomb = obj.GetComponent<BombController>();
        obj.SetActive(false);
        pool.Enqueue(bomb);
        return bomb;
    }


    public BombController Get(Vector3 position)
    {
        BombController bomb = pool.Count > 0 ? pool.Dequeue() : CreateBomb();   //crea solo se pool vuoto
        bomb.transform.position = position; //Assegna alla bomba l'attuale posizione del player
        bomb.gameObject.SetActive(true);
        bomb.Initialize();
        return bomb;
    }

    public void ReturnToPool(BombController bomb)
    {
        bomb.gameObject.SetActive(false);
        pool.Enqueue(bomb);
    }

}
