using System;
using UnityEngine;

public enum E_Map
{
    Spring,
    Winter
}

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    
    private GameObject currentMap = null;
    private MapComponent mapComponent = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        LoadMap(GameSession.SelectedMap);
    }

    public void LoadMap(E_Map eMap)
    {
        if (currentMap != null)
        {
            Destroy(currentMap);
            currentMap = null;
        }

        GameObject mapPrefab = Resources.Load<GameObject>(GetMapPath(eMap));

        if (mapPrefab != null)
        {
            currentMap = Instantiate(mapPrefab);
        }
    }

    private string GetMapPath(E_Map eMap)
    {
        string path = "";
        
        switch (eMap)
        {
            case E_Map.Spring:
                path = "Prefabs/Maps/MapSpring";
                break;
            case E_Map.Winter:
                path = "Prefabs/Maps/MapWinter";
                break;
        }

        return path;
    }
    
    public Vector2Int[] GetPlayerSpawnPositions()
    {
        if (currentMap == null) return null;
        
        mapComponent = currentMap.GetComponent<MapComponent>();

        return mapComponent.GetPlayerSpawnPos();
    }
    
}
