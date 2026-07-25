using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;


/*
 * Gestisce una bomba sincronizzata nella partita multiplayer
 *
 * Responsabilità:
 * - leggere i dati ricevuti durante l'istanziazione Photon
 * - disabilitare inizialmente il collider
 * - permettere al player di uscire dalla cella della bomba
 * - gestire il timer solamente sul Master Client
 * - notificare il PhotonBombHandler proprietario
 * - distruggere la bomba su tutti i client
 *
 */




[DisallowMultipleComponent]
[RequireComponent(typeof(PhotonView))]
public sealed class PhotonBombController : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [Header("Bomb Collider")]
    [Tooltip("Collider utilizzato dalla bomba per bloccare la cella.")]
    [SerializeField] private Collider2D bombCollider;

    [Header("Fallback")]
    [Tooltip("Durata utilizzata se i dati Photon non contengono un timer valido.")]
    [SerializeField] [Min(0.1f)] private float fallbackFuseDuration = 3f;

    [Header("Game Settings")]
    [SerializeField] private GameData gameData;

    private int ownerPlayerViewId = -1;
    private float fuseDuration;

    private Coroutine fuseCoroutine;
    private Coroutine colliderCoroutine;


    // Recupera il collider e lo disabilita finché tutti i player non hanno lasciato la cella.
    private void Awake()
    {
        if (bombCollider == null)
        {
            bombCollider = GetComponent<Collider2D>();
        }

        if (bombCollider != null)
        {
            bombCollider.enabled = false;
        }
    }


    // Riceve i dati passati dal Master durante PhotonNetwork.InstantiateRoomObject
    public void OnPhotonInstantiate(PhotonMessageInfo messageInfo)
    {
        object[] instantiationData = photonView.InstantiationData;
        
        if (instantiationData == null || instantiationData.Length < 2)
        {
            Debug.LogError("[PhotonBombController] " + "InstantiationData mancanti o incomplete.", this);
            return;
        }

        try
        {
            ownerPlayerViewId = Convert.ToInt32(instantiationData[0]);

            fuseDuration = Convert.ToSingle(instantiationData[1]);
        }
        catch (Exception exception)
        {
            Debug.LogError("[PhotonBombController] " + "Errore durante la lettura degli " + $"InstantiationData: {exception.Message}", this);
            return;
        }

        if (fuseDuration <= 0f)
        {
            fuseDuration = fallbackFuseDuration;
        }

        // La bomba è un Room Object: soltanto il Master Client ne gestisce collider, timer e distruzione
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        colliderCoroutine = StartCoroutine(EnableColliderWhenCellIsClear());

        fuseCoroutine = StartCoroutine(FuseRoutine());

        Debug.Log(
            "[PhotonBombController] " +
            $"Bomba inizializzata. " +
            $"Owner Player ViewID: {ownerPlayerViewId}. " +
            $"Fuse: {fuseDuration} secondi."
        );
    }


    // Attende che nessun personaggio si trovi più sulla cella della bomba e abilita il collider su tutti
    private IEnumerator EnableColliderWhenCellIsClear()
    {
        while (IsAnyPlayerOnBombCell())
        {
            yield return null;
        }

        photonView.RPC(nameof(RPC_SetColliderEnabled), RpcTarget.All, true);

        colliderCoroutine = null;
    }



    // Controlla sul Master se almeno un personaggio occupa ancora la stessa cella della bomba
    private bool IsAnyPlayerOnBombCell()
    {
        if (gameData == null)
        {
            return false;
        }

        PhotonPlayerController[] players = FindObjectsByType<PhotonPlayerController>(FindObjectsSortMode.None);

        Vector3 bombCell = GridUtils.AdjustPosition(transform.position, gameData.fCellSize);

        foreach (PhotonPlayerController player in players)
        {
            if (player == null)
            {
                continue;
            }

            Vector3 playerCell = GridUtils.AdjustPosition(player.transform.position, gameData.fCellSize);

            if (Vector3.Distance(playerCell, bombCell) < 0.1f
            )
            {
                return true;
            }
        }

        return false;
    }


    // Abilita o disabilita il collider della bomba nello stesso momento su tutti i client
    [PunRPC]
    private void RPC_SetColliderEnabled(bool enabled)
    {
        if (bombCollider != null)
        {
            bombCollider.enabled = enabled;
        }
    }


    // Attende il tempo di innesco e termina la bomba esclusivamente sul Master Client
    private IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(fuseDuration);

        FinishBomb();
    }


    // Notifica il player che la bomba è terminata e la distrugge in maniera sincronizzata
    private void FinishBomb()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        PhotonView ownerPlayerView = PhotonView.Find(ownerPlayerViewId);

        if (ownerPlayerView != null)
        {
            PhotonBombHandler bombHandler = ownerPlayerView.GetComponent<PhotonBombHandler>();
            bombHandler?.NotifyBombFinished();
        }

        PhotonNetwork.Destroy(gameObject);
    }


    // Interrompe le coroutine locali quando la bomba viene distrutta o disabilitata.
    private void OnDisable()
    {
        if (fuseCoroutine != null)
        {
            StopCoroutine(fuseCoroutine);
            fuseCoroutine = null;
        }

        if (colliderCoroutine != null)
        {
            StopCoroutine(colliderCoroutine);
            colliderCoroutine = null;
        }
    }



}
