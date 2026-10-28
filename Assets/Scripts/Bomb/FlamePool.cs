using System.Collections.Generic;
using UnityEngine;

public class FlamePool : MonoBehaviour
{
    public static FlamePool Instance { get; private set; }

    [SerializeField] private GameObject flamePrefab;
    [SerializeField] private int iPoolSize = 20;

    private Queue<FlameController> pool = new Queue<FlameController>();

    //[AI] Registro delle fiamme attualmente accese (fuori dal pool)
    private readonly List<FlameController> activeFlames = new List<FlameController>();
    public IReadOnlyList<FlameController> ActiveFlames => activeFlames;

    private void Awake()
    {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        for(int i = 0; i < iPoolSize; i++) { pool.Enqueue(CreateFlame()); }
    }

    //[AI] Crea una fiamma spenta, senza accodarla (vedi nota in BombPool.CreateBomb)
    FlameController CreateFlame()
    {
        GameObject obj = Instantiate(flamePrefab, transform);
        FlameController flame = obj.GetComponent<FlameController>();
        obj.SetActive(false);
        return flame;
    }

    public FlameController Get(Vector3 v3Position)
    {
        FlameController flame = pool.Count > 0 ? pool.Dequeue() : CreateFlame();

        if(flame == null) { Debug.LogError("Errore nella creazione della fiamma"); return null; }

        flame.transform.position = v3Position;
        flame.gameObject.SetActive(true);
        activeFlames.Add(flame);            //[AI] registra la fiamma come attiva
        return flame;
    }

    public void ReturnToPool(FlameController flame)
    {
        activeFlames.Remove(flame);         //[AI] non è più attiva
        flame.gameObject.SetActive(false);
        pool.Enqueue(flame);
    }


}