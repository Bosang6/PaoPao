using UnityEngine;

public class ExplosionManager : MonoBehaviour
{

    [Header("Game Settings")]
    [SerializeField] private GameData gameData;
    public static ExplosionManager Instance { get; private set; }

    private void Awake()
    {
        //Singleton
        if(Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void OnExplode(Vector3 v3Origin, ExplosionData data)
    {
        //Invoca le esplosioni al posto della bomba e ai lati
        SpawnFlame(v3Origin, data, FlameType.Center);

        VerifyFlammable(v3Origin, Vector2.up, data);
        VerifyFlammable(v3Origin, Vector2.down, data);
        VerifyFlammable(v3Origin, Vector2.left, data);
        VerifyFlammable(v3Origin, Vector2.right, data);
    }

    private void VerifyFlammable(Vector3 v3Origin, Vector2 v2Direction, ExplosionData data)
    {
        //bool isHor = v2Direction.x != 0;
        int dirToInt = 0; // 0: Up, 1: Right, 2: Down, 3: Left
        if (v2Direction.x > 0 && v2Direction.y == 0)
        {
            dirToInt = 1;
        }
        else if (v2Direction.x < 0 && v2Direction.y == 0)
        {
            dirToInt = 3;
        }
        else if (v2Direction.x == 0 && v2Direction.y > 0)
        {
            dirToInt = 0;
        }
        else
        {
            dirToInt = 2;
        }

        FlameType type; //Tipo della fiamma (HorizontalMid, HorizontalEnd, VerticalMid, VerticalEnd) 
        
        //Dimensione tile (leggermente ridotta per evitare di toccare tile adiacenti)
        Vector2 v2BoxSize = new Vector2(gameData.fCellSize * 0.9f, gameData.fCellSize * 0.9f);    

        for(int i = 1; i <= data.iRange; i++)
        {
            //Calcola il centro della cella colpita dalle fiamme
            Vector3 v3CellCenter = v3Origin + new Vector3(v2Direction.x, v2Direction.y, 0) * (i * gameData.fCellSize);

            //Se colpisce un muro indistruttibile, non fa nulla
            if(Physics2D.OverlapBox(v3CellCenter, v2BoxSize, 0f, data.lmWallLayer)) { return; }

            //Se colpisce un muro distruttibile, spawna la fiamma e ferma il raggio
            if (Physics2D.OverlapBox(v3CellCenter, v2BoxSize, 0f, data.lmBreakableLayer)) {
                //type = isHor ? FlameType.HorizontalEnd : FlameType.VerticalEnd;
                if (dirToInt == 0)
                {
                    type = FlameType.VerticalTopEnd;
                }
                else if (dirToInt == 1)
                {
                    type = FlameType.HorizontalRightEnd;
                }
                else if (dirToInt == 2)
                {
                    type = FlameType.VerticalBottomEnd;
                }
                else
                {
                    type = FlameType.HorizontalLeftEnd;
                }
                SpawnFlame(v3CellCenter, data, type);
                return;
            }

            //La cella � libera / player -> mostra la fiamma e prosegue col cast (se data.iRange > 1)

            if (i == data.iRange)
            {
                //type = isHor ? FlameType.HorizontalEnd : FlameType.VerticalEnd;
                if (dirToInt == 0)
                {
                    type = FlameType.VerticalTopEnd;
                }
                else if (dirToInt == 1)
                {
                    type = FlameType.HorizontalRightEnd;
                }
                else if (dirToInt == 2)
                {
                    type = FlameType.VerticalBottomEnd;
                }
                else
                {
                    type = FlameType.HorizontalLeftEnd;
                }
            }
            else
            {
                //type = isHor ? FlameType.HorizontalMid : FlameType.VerticalMid;
                if (dirToInt == 0)
                {
                    type = FlameType.VerticalTopMid;
                }
                else if (dirToInt == 1)
                {
                    type = FlameType.HorizontalRightMid;
                }
                else if (dirToInt == 2)
                {
                    type = FlameType.VerticalBottomMid;
                }
                else
                {
                    type = FlameType.HorizontalLeftMid;
                }
            }
            
            SpawnFlame(v3CellCenter, data, type);
        }
    }

    private void SpawnFlame(Vector3 v3Pos, ExplosionData data, FlameType type = FlameType.Center)
    {
        FlameController flame = FlamePool.Instance.Get(v3Pos);
        if (flame == null) return;
        flame.Initialize(data, type);
    }
}
