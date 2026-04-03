using System.Collections;
using UnityEngine;

public class BombController : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private float fTimeToExplode = 3f;

    public event System.Action OnBombReturned;

    private Coroutine explodingCoroutine;

    public void Initialize()    //Chiamato da BombPool.Get() ad ogni utilizzo della bomba
    {
        if(explodingCoroutine != null) { StopCoroutine(explodingCoroutine); }

        explodingCoroutine = StartCoroutine(Exploding());
    }

    private IEnumerator Exploding()
    {
        yield return new WaitForSeconds(fTimeToExplode);
        Explode();
    }

    private void Explode()
    {
        Debug.Log($"Bomba esplosa in {transform.position}");
        OnBombReturned?.Invoke();
        OnBombReturned = null; // reset per evitare listener doppi al riutilizzo
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
