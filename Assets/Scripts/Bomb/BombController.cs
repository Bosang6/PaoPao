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
    private readonly List<Collider2D> ignoredPlayerColliders = new List<Collider2D>();
    private List<Transform> playersTransforms;
    private Collider2D bombCollider;

    public bool IsHuman = false;
    private float explodeAtTime;

    public Vector3 Position => transform.position;
    public float TimeToExplode => Mathf.Max(0f, explodeAtTime - Time.time);
    public ExplosionData Data => explosionData;

    private void Awake() { bombCollider = GetComponent<Collider2D>(); }

    public void Initialize(List<Transform> players, bool isHuman)
    {
        RestoreIgnoredCollisions();
        playersTransforms = players;
        IsHuman = isHuman;
        explodeAtTime = Time.time + explosionData.fTimeToExplode;

        // Keep the collider active and ignore only players already overlapping it.
        // Disabling/re-enabling a Collider2D can clear IgnoreCollision state.
        if (playersTransforms != null)
        {
            foreach (Transform player in playersTransforms)
            {
                if (player == null) continue;
                Collider2D playerCollider = player.GetComponent<Collider2D>();
                if (playerCollider == null) playerCollider = player.GetComponentInChildren<Collider2D>();
                if (playerCollider == null || !PlayerOverlapsBomb(playerCollider)) continue;

                Physics2D.IgnoreCollision(bombCollider, playerCollider, true);
                ignoredPlayerColliders.Add(playerCollider);
                StartCoroutine(RestoreCollisionWhenClear(playerCollider));
            }
        }
        if (explodingCoroutine != null) StopCoroutine(explodingCoroutine);
        explodingCoroutine = StartCoroutine(Exploding());
    }

    private IEnumerator RestoreCollisionWhenClear(Collider2D playerCollider)
    {
        while (bombCollider != null && bombCollider.enabled &&
               playerCollider != null && playerCollider.enabled && PlayerOverlapsBomb(playerCollider))
            yield return null;

        if (bombCollider != null && playerCollider != null)
            Physics2D.IgnoreCollision(bombCollider, playerCollider, false);
        ignoredPlayerColliders.Remove(playerCollider);
    }

    private bool PlayerOverlapsBomb(Collider2D playerCollider)
    {
        return playerCollider != null && playerCollider.bounds.Intersects(GetBombBounds());
    }

    private Bounds GetBombBounds()
    {
        if (bombCollider is BoxCollider2D box)
        {
            Vector3 center = transform.TransformPoint(box.offset);
            Vector3 size = Vector3.Scale(box.size, transform.lossyScale);
            return new Bounds(center, new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y),
                Mathf.Max(0.01f, Mathf.Abs(size.z))));
        }

        return new Bounds(transform.position, Vector3.one * gameData.fCellSize * 0.9f);
    }

    private void RestoreIgnoredCollisions()
    {
        foreach (Collider2D playerCollider in ignoredPlayerColliders)
            if (bombCollider != null && playerCollider != null)
                Physics2D.IgnoreCollision(bombCollider, playerCollider, false);
        ignoredPlayerColliders.Clear();
    }

    private IEnumerator Exploding()
    {
        yield return new WaitForSeconds(explosionData.fTimeToExplode);
        Explode();
    }

    private void Explode()
    {
        OnBombReturned?.Invoke();
        OnBombReturned = null;
        if (IsHuman && AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioEvent.BombExplosion);
        ExplosionManager.Instance.OnExplode(transform.position, explosionData, IsHuman);
        BombPool.Instance.ReturnToPool(this);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        explodingCoroutine = null;
        RestoreIgnoredCollisions();
    }
}
