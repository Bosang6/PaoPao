using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;


/*
 * Gestisce la propagazione grafica delle esplosioni multiplayer
 *
 * Il Master Client:
 * - calcola le celle raggiunte dall'esplosione
 * - controlla muri indistruttibili e distruttibili
 * - comunica a tutti i client posizione e tipo delle fiamme
 *
 * Ogni client:
 * - mostra localmente le stesse fiamme
 * - utilizza un pool per evitare continue Instantiate e Destroy
 *
 */

[DisallowMultipleComponent]
[RequireComponent(typeof(PhotonView))]
public sealed class PhotonExplosionManager : MonoBehaviourPun
{

    public static PhotonExplosionManager Instance
    {
        get;
        private set;
    }


    [Header("Game Settings")]
    [SerializeField] private GameData gameData;

    [Header("Flame Visual")]
    [SerializeField] private GameObject flameVisualPrefab;

    [Header("Breakable Wall")]
    [SerializeField] private GameObject breakEffectPrefab;

    [SerializeField] [Min(1)] private int initialPoolSize = 20;

    private readonly Queue<PhotonFlameVisual> flamePool = new Queue<PhotonFlameVisual>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        PrewarmPool();
    }


    // Crea preventivamente un numero iniziale di fiamme grafiche
    private void PrewarmPool()
    {
        if (flameVisualPrefab == null)
        {
            Debug.LogError("[PhotonExplosionManager] " + "Flame Visual Prefab non assegnato.", this);
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateFlameVisual();
        }
    }


    // Crea una nuova fiamma grafica e la inserisce nel pool
    private PhotonFlameVisual CreateFlameVisual()
    {
        GameObject flameObject = Instantiate(flameVisualPrefab, transform);

        PhotonFlameVisual flameVisual = flameObject.GetComponent<PhotonFlameVisual>();

        if (flameVisual == null)
        {
            Debug.LogError("[PhotonExplosionManager] " + $"Il prefab '{flameVisualPrefab.name}' " + "non possiede PhotonFlameVisual.", flameObject);

            Destroy(flameObject);

            return null;
        }

        flameObject.SetActive(false);

        flamePool.Enqueue(flameVisual);

        return flameVisual;
    }


    // Viene chiamato esclusivamente dal Master Client quando il timer di una bomba termina
    public void Explode(Vector3 origin, ExplosionData explosionData, int ownerPlayerViewId)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (gameData == null)
        {
            Debug.LogError("[PhotonExplosionManager] GameData non assegnato.", this);

            return;
        }

        if (explosionData == null)
        {
            Debug.LogError("[PhotonExplosionManager] ExplosionData mancante.", this);

            return;
        }

        // Il ViewID non viene ancora utilizzato nel passaggio grafico.
        // Verrà utilizzato successivamente per danni e uccisioni.
        _ = ownerPlayerViewId;

        ActivateFlameCell(origin, FlameType.Center, explosionData, ownerPlayerViewId);

        VerifyDirection(origin, Vector2.up, explosionData, ownerPlayerViewId);

        VerifyDirection(origin, Vector2.down, explosionData, ownerPlayerViewId);

        VerifyDirection(origin, Vector2.left, explosionData, ownerPlayerViewId);

        VerifyDirection(origin, Vector2.right, explosionData, ownerPlayerViewId);
    }


    // Replica la logica di propagazione già presente nell'ExplosionManager del Single Player
    private void VerifyDirection(Vector3 origin, Vector2 direction, ExplosionData explosionData, int ownerPlayerViewId)
    {
        Vector2 boxSize = new Vector2( gameData.fCellSize * 0.9f, gameData.fCellSize * 0.9f);

        for (int distance = 1; distance <= explosionData.iRange; distance++)
        {
            Vector3 cellCenter =
                origin +
                new Vector3(
                    direction.x,
                    direction.y,
                    0f
                ) *
                (distance * gameData.fCellSize);

            // Il muro indistruttibile blocca la propagazione senza mostrare alcuna fiamma sulla propria cella
            Collider2D wall = Physics2D.OverlapBox(
                cellCenter,
                boxSize,
                0f,
                explosionData.lmWallLayer
            );

            if (wall != null)
            {
                return;
            }

            Collider2D breakableWall =
                Physics2D.OverlapBox(
                    cellCenter,
                    boxSize,
                    0f,
                    explosionData.lmBreakableLayer
                );

            bool hasBreakableWall = breakableWall != null;

            bool isEnd = hasBreakableWall || distance == explosionData.iRange;

            FlameType flameType = GetFlameType(direction, isEnd);

            ActivateFlameCell(cellCenter, flameType, explosionData, ownerPlayerViewId);

            // Il muro distruttibile riceve la fiamma finale e interrompe la propagazione.
            if (hasBreakableWall)
            {
                BroadcastBreakableTileDestruction(cellCenter, explosionData.lmBreakableLayer.value);
                return;
            }
        }
    }


    // Restituisce lo stesso FlameType utilizzato dall'ExplosionManager del Single Player
    private FlameType GetFlameType(Vector2 direction, bool isEnd)
    {
        if (direction == Vector2.up)
        {
            return isEnd ? FlameType.VerticalTopEnd : FlameType.VerticalTopMid;
        }

        if (direction == Vector2.down)
        {
            return isEnd ? FlameType.VerticalBottomEnd : FlameType.VerticalBottomMid;
        }

        if (direction == Vector2.right)
        {
            return isEnd ? FlameType.HorizontalRightEnd : FlameType.HorizontalRightMid;
        }

        return isEnd ? FlameType.HorizontalLeftEnd : FlameType.HorizontalLeftMid;
    }


    // Mostra la fiamma su tutti i client e avvia sul Master il controllo del danno per la durata della fiamma
    private void ActivateFlameCell(Vector3 position, FlameType flameType, ExplosionData explosionData, int ownerPlayerViewId)
    {
        BroadcastFlame(position, flameType, explosionData.fFlameDuration);

        StartCoroutine(DamageRoutine(position, explosionData, ownerPlayerViewId));
    }

    // Replica il comportamento del FlameController del Single Player, controlla la cella ogni 0,1 secondi finche la fiamma è attiva
    private IEnumerator DamageRoutine(Vector3 flamePosition, ExplosionData explosionData, int ownerPlayerViewId)
    {
        const float checkInterval = 0.1f;

        float elapsedTime = 0f;

        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (elapsedTime < explosionData.fFlameDuration)
        {
            ApplyDamageInCell(flamePosition, explosionData, ownerPlayerViewId);
            yield return wait;
            elapsedTime += checkInterval;
        }
    }

    // Cerca sul Master i personaggi presenti nella cella e chiede al loro controller di applicare il danno
    private void ApplyDamageInCell(Vector3 flamePosition, ExplosionData explosionData, int ownerPlayerViewId)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        Vector2 boxSize = new Vector2( gameData.fCellSize * 0.9f, gameData.fCellSize * 0.9f);

        Collider2D[] playerColliders =
            Physics2D.OverlapBoxAll(
                flamePosition,
                boxSize,
                0f,
                explosionData.lmPlayerLayer
            );

        foreach (Collider2D playerCollider
                 in playerColliders)
        {
            if (playerCollider == null)
            {
                continue;
            }

            PhotonPlayerController player = playerCollider.GetComponent<PhotonPlayerController>();

            if (player == null)
            {
                player = playerCollider.GetComponentInParent<PhotonPlayerController>();
            }

            if (player == null)
            {
                continue;
            }

            player.TryApplyExplosionDamage(explosionData, ownerPlayerViewId);
        }
    }



    // Invia a tutti i client la pos, il tipo e la durata della singola fiamma
    private void BroadcastFlame(Vector3 position, FlameType flameType, float duration)
    {
        photonView.RPC(
            nameof(RPC_ShowFlame),
            RpcTarget.AllViaServer,
            position,
            (int)flameType,
            duration
        );
    }


    // Ogni client genera localmente la stessa fiamma grafica
    [PunRPC]
    private void RPC_ShowFlame(Vector3 position, int flameTypeValue, float duration)
    {
        if (flameTypeValue < (int)FlameType.Center || flameTypeValue > (int)FlameType.VerticalBottomEnd)
        {
            Debug.LogError("[PhotonExplosionManager] " + $"FlameType non valido: {flameTypeValue}.", this);
            return;
        }

        PhotonFlameVisual flameVisual = GetFlameVisual();

        if (flameVisual == null)
        {
            return;
        }

        flameVisual.transform.position = position;

        flameVisual.gameObject.SetActive(true);

        flameVisual.Initialize((FlameType)flameTypeValue, duration, ReturnFlameVisual);
    }


    // Recupera una fiamma dal pool oppure ne crea una nuova
    private PhotonFlameVisual GetFlameVisual()
    {
        while (flamePool.Count > 0)
        {
            PhotonFlameVisual flameVisual = flamePool.Dequeue();

            if (flameVisual != null)
            {
                return flameVisual;
            }
        }

        PhotonFlameVisual createdFlame = CreateFlameVisual();

        if (createdFlame == null)
        {
            return null;
        }

        // CreateFlameVisual inserisce il nuovo elemento nel pool.
        return flamePool.Dequeue();
    }


    // Disattiva la fiamma e la rende nuovamente disponibile
    private void ReturnFlameVisual(
        PhotonFlameVisual flameVisual
    )
    {
        if (flameVisual == null)
        {
            return;
        }

        flameVisual.gameObject.SetActive(false);

        flamePool.Enqueue(flameVisual);
    }



    // Comunica a tutti i client quale cella distruttibile
    // deve essere rimossa dalla mappa.
    private void BroadcastBreakableTileDestruction(
        Vector3 worldPosition,
        int breakableLayerMask
    )
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        photonView.RPC(
            nameof(RPC_DestroyBreakableTile),
            RpcTarget.AllViaServer,
            worldPosition,
            breakableLayerMask
        );
    }


    // Cerca localmente la Tilemap che occupa la cella indicata e rimuove la stessa tile su ogni client
    [PunRPC]
    private void RPC_DestroyBreakableTile(Vector3 worldPosition, int breakableLayerMask)
    {
        if (gameData == null)
        {
            Debug.LogError("[PhotonExplosionManager] " + "GameData non assegnato durante la distruzione della tile.", this);
            return;
        }

        Vector2 boxSize = new Vector2(gameData.fCellSize * 0.9f, gameData.fCellSize * 0.9f);

        Collider2D breakableCollider =
            Physics2D.OverlapBox(
                worldPosition,
                boxSize,
                0f,
                breakableLayerMask
            );

        if (breakableCollider == null)
        {
            // La tile potrebbe essere già stata eliminata da un'altra esplosione ricevuta nello stesso momento
            return;
        }

        Tilemap breakableTilemap = breakableCollider.GetComponent<Tilemap>();

        if (breakableTilemap == null)
        {
            breakableTilemap = breakableCollider.GetComponentInParent<Tilemap>();
        }

        if (breakableTilemap == null)
        {
            Debug.LogError("[PhotonExplosionManager] " + $"Il collider '{breakableCollider.name}' " + "non appartiene a una Tilemap.", breakableCollider);
            return;
        }

        Vector3Int cellPosition = breakableTilemap.WorldToCell(worldPosition);

        TileBase tile = breakableTilemap.GetTile(cellPosition);

        if (tile == null)
        {
            return;
        }

        Vector3 effectPosition = breakableTilemap.GetCellCenterWorld(cellPosition);

        breakableTilemap.SetTile(cellPosition, null);

        if (breakEffectPrefab != null)
        {
            Instantiate(breakEffectPrefab, effectPosition, Quaternion.identity);
        }
    }






    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }








}
