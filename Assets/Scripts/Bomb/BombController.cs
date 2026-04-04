using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private ExplosionData explosionData;

    [Header("Game Settings")]
    [SerializeField] private GameData gameData;

    public event System.Action OnBombReturned;
    private Coroutine explodingCoroutine;

    private List<Transform> playersTransforms;
    private Collider2D bombCollider;

    private void Awake() { bombCollider = GetComponent<Collider2D>(); }

    public void Initialize(List<Transform> players)    //Chiamato da BombPool.Get() ad ogni utilizzo della bomba
    {
        this.playersTransforms = players;

        if (explodingCoroutine != null) { StopCoroutine(explodingCoroutine); }
        explodingCoroutine = StartCoroutine(Exploding());
        StartCoroutine(EnableColliderWhenClear(transform.position));
    }

    private IEnumerator EnableColliderWhenClear(Vector3 bombCell)
    {
        //Si parte col collider disattivato
        bombCollider.enabled = false;
        
        //Si aspetta che non ci sia più nessun player sulla bomba
        while (AnyPlayerOnCell(bombCell)) { yield return null; }

        //Il collider diventa attivo
        bombCollider.enabled = true;
    }

    private bool AnyPlayerOnCell(Vector3 cell)
    {
        foreach (Transform player in playersTransforms)
        {
            //Si controlla se ci sono player sulla stessa tile della bomba
            Vector3 playerCell = GridUtils.AdjustPosition(player.position, gameData.fCellSize);
            if (Vector3.Distance(playerCell, cell) < 0.1f) { return true; }
        }
        return false;
    }

    private IEnumerator Exploding()
    {
        yield return new WaitForSeconds(explosionData.fTimeToExplode);
        Explode();
    }

    private void Explode()
    {
        Debug.Log($"Bomba esplosa in {transform.position}");
        OnBombReturned?.Invoke();
        OnBombReturned = null;

        /*  Invoca un evento di tipo event System.Action. Il simbolo "?." significa "invoca
         *  solo se qualcuno sta ascoltando" (quindi se ci sono listener registrati).
         *  Successivamente, si de-registra dall'evento (poiché la bomba torna nel pool).
         *  La prossima volta che la bomba verrà estratta dal pool, gli verrà agganciato
         *  un nuovo listener
         *  La sintassi = null permette di chiudere il canale di comunicazione coi listener
         */

        //Delega l'esplosione all'ExplosionManager
        ExplosionManager.Instance.OnExplode(transform.position, explosionData);
        BombPool.Instance.ReturnToPool(this);
    }

    //Se il GameObject viene disattivato prima dello scadere del timer
    void OnDisable()
    {
        if (explodingCoroutine != null)
        {
            StopCoroutine(explodingCoroutine);
            explodingCoroutine = null;
        }
    }
}
