using Photon.Pun;
using UnityEngine;


/*
 * Gestisce la richiesta di piazzamento delle bombe per un personaggio Multiplayer
 * 
 * Responsabilità:
 * - ricevere la richiesta dal proprietario del personaggio
 * - inviarla al Master Client
 * - verificare sul Master il limite massimo di bombe
 * - creare una sola bomba sincronizzata nella Room
 * - mantenere il conteggio autoritativo delle bombe attive
 * 
 */



[DisallowMultipleComponent]
[RequireComponent(typeof(PhotonView))]
public sealed class PhotonBombHandler : MonoBehaviourPun
{
    [Header("Network Bomb")]
    [Tooltip("Prefab della bomba utilizzata nella partita multiplayer.")]
    [SerializeField] private GameObject networkBombPrefab;

    private string networkBombPrefabId;
    private bool isPrefabRegistered;

    private GameData gameData;
    private CharacterData characterData;

    // Questo valore viene utilizzato soltanto dalla copia presente sul Master Client
    private int activeBombCount;

    private bool isInitialized;

    // Registra il prefab della bomba nella cache del DefaultPool, così Photon può recuperarlo usando il suo nome.
    private void Awake()
    {
        RegisterNetworkBombPrefab();
    }


    // Riceve i dati runtime creati dal PhotonPlayerController e prepara il componente per le richieste di piazzamento
    public void Initialize(GameData runtimeGameData, CharacterData runtimeCharacterData)
    {
        gameData = runtimeGameData;
        characterData = runtimeCharacterData;

        activeBombCount = 0;

        isInitialized = gameData != null && characterData != null && characterData.explosionData != null;

        if (!isInitialized)
        {
            Debug.LogError("[PhotonBombHandler] " + "Inizializzazione fallita: GameData, CharacterData o ExplosionData mancanti.", this);
            return;
        }

        if (!isPrefabRegistered)
        {
            Debug.LogError("[PhotonBombHandler] " + "Il prefab della bomba non è stato registrato correttamente.", this);
            isInitialized = false;
        }

    }



    // Viene chiamato dal PhotonPlayerController quando il giocatore locale o una AI richiedono una bomba.
    public void TryRequestBomb()
    {
        if (!isInitialized || !isPrefabRegistered)
        {
            return;
        }

        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("[PhotonBombHandler] " + "Impossibile piazzare la bomba: il client non è dentro una Room.", this);
            return;
        }

        // Solo il client che controlla questo PhotonView può inviare la richiesta-
        // - player umano: il relativo client
        // - AI: il Master Client
        if (!photonView.IsMine)
        {
            return;
        }

        photonView.RPC(nameof(RPC_RequestPlaceBomb), RpcTarget.MasterClient);
    }


    // Riceve sul Master Client la richiesta di piazzamento e verifica che provenga dal proprietario del personaggio
    [PunRPC]
    private void RPC_RequestPlaceBomb(PhotonMessageInfo messageInfo)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (!isInitialized)
        {
            Debug.LogWarning("[PhotonBombHandler] " + "Richiesta ricevuta prima dell'inizializzazione.", this);
            return;
        }

        if (messageInfo.Sender == null || messageInfo.Sender.ActorNumber != photonView.OwnerActorNr)
        {
            Debug.LogWarning("[PhotonBombHandler] " + "Richiesta bomba rifiutata: il mittente non controlla questo personaggio.", this);
            return;
        }

        if (activeBombCount >= characterData.maxBombs)
        {
            return;
        }

        Vector3 bombPosition = GridUtils.AdjustPosition(transform.position, gameData.fCellSize);

        if (IsBombAlreadyOnCell(bombPosition))
        {
            return;
        }

        object[] instantiationData =
        {
            photonView.ViewID,
            characterData.explosionData.fTimeToExplode
        };

        GameObject spawnedBomb =
            PhotonNetwork.InstantiateRoomObject(
                networkBombPrefabId,
                bombPosition,
                Quaternion.identity,
                0,
                instantiationData
            );

        if (spawnedBomb == null)
        {
            Debug.LogError("[PhotonBombHandler] " + "Photon non è riuscito a creare la bomba.", this);
            return;
        }

        activeBombCount++;

        Debug.Log(
            "[PhotonBombHandler] " +
            $"Bomba creata per Player ViewID " +
            $"{photonView.ViewID}. " +
            $"Bombe attive: {activeBombCount}/" +
            $"{characterData.maxBombs}."
        );
    }


    // Controlla sul Master Client che nella cella non sia già presente un'altra bomba multiplayer
    private bool IsBombAlreadyOnCell(Vector3 cellPosition)
    {
        Vector2 boxSize = new Vector2(gameData.fCellSize * 0.5f, gameData.fCellSize * 0.5f);

        Collider2D[] colliders = Physics2D.OverlapBoxAll(cellPosition, boxSize, 0f);

        foreach (Collider2D currentCollider in colliders)
        {
            if (currentCollider != null && currentCollider.GetComponent<PhotonBombController>() != null)
            {
                return true;
            }
        }

        return false;
    }



    // Riduce il contatore autoritativo quando una bomba termina il proprio ciclo e viene distrutta dal Master
    public void NotifyBombFinished()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        activeBombCount = Mathf.Max(0, activeBombCount - 1);

        Debug.Log(
            "[PhotonBombHandler] " +
            $"Bomba terminata per Player ViewID " +
            $"{photonView.ViewID}. " +
            $"Bombe attive: {activeBombCount}/" +
            $"{characterData.maxBombs}."
        );
    }


    // Registra il prefab della bomba nel DefaultPool di Photon. Ogni client deve possedere la stessa associazione tra nome e prefab
    private void RegisterNetworkBombPrefab()
    {
        isPrefabRegistered = false;
        networkBombPrefabId = null;

        if (networkBombPrefab == null)
        {
            Debug.LogError("[PhotonBombHandler] NetworkBomb Prefab non assegnato.", this);
            return;
        }

        if (networkBombPrefab.GetComponent<PhotonView>() == null)
        {
            Debug.LogError("[PhotonBombHandler] " + $"Il prefab '{networkBombPrefab.name}' non possiede un PhotonView.", networkBombPrefab);
            return;
        }

        if (!(PhotonNetwork.PrefabPool is DefaultPool defaultPool))
        {
            Debug.LogError("[PhotonBombHandler] " + "Il PrefabPool corrente non è il DefaultPool di Photon.", this);
            return;
        }

        networkBombPrefabId = networkBombPrefab.name;

        if (defaultPool.ResourceCache.TryGetValue(networkBombPrefabId, out GameObject cachedPrefab))
        {
            if (cachedPrefab != networkBombPrefab)
            {
                Debug.LogError("[PhotonBombHandler] " + $"Esiste già un altro prefab registrato con ID " + $"'{networkBombPrefabId}'.", this);
                return;
            }

            isPrefabRegistered = true;
            return;
        }

        defaultPool.ResourceCache.Add(networkBombPrefabId, networkBombPrefab);

        isPrefabRegistered = true;

        Debug.Log("[PhotonBombHandler] " + $"Prefab bomba registrato con ID '{networkBombPrefabId}'.");
    }



}
