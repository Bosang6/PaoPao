using UnityEngine;

public class TileBreakEvent : MonoBehaviour
{
    [SerializeField] private AudioEvent breakSound;

    // Il suono parte ad inizio animazione
    private void OnEnable()
    {
        AudioManager.Instance.PlaySFX(breakSound);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}