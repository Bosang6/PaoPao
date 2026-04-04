using System.Collections.Generic;
using UnityEngine;

public class FlamePool : MonoBehaviour
{
    public static FlamePool Instance { get; private set; }

    [SerializeField] private GameObject flamePrefab;
    [SerializeField] private int iPoolSize = 20;

    private Queue<FlameController> pool = new Queue<FlameController>();

    private void Awake()
    {
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        for(int i = 0; i < iPoolSize; i++) { CreateFlame(); }
    }
    FlameController CreateFlame()
    {
        GameObject obj = Instantiate(flamePrefab, transform);
        FlameController flame = obj.GetComponent<FlameController>();
        obj.SetActive(false);
        pool.Enqueue(flame);
        return flame;
    }

    public FlameController Get(Vector3 v3Position)
    {
        FlameController flame = pool.Count > 0 ? pool.Dequeue() : CreateFlame();

        if(flame == null) { Debug.LogError("Errore nella creazione della fiamma"); return null; }

        flame.transform.position = v3Position;
        flame.gameObject.SetActive(true);
        return flame;
    }

    public void ReturnToPool(FlameController flame)
    {
        flame.gameObject.SetActive(false);
        pool.Enqueue(flame);
    }


}
