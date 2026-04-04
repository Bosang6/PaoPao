using UnityEngine;

public class PlayerHealth : MonoBehaviour, IExplosionReceiver
{
    [SerializeField] private int iHealth;

    void Start()
    {
        iHealth = 3;
    }

    public void OnHitByExplosion(ExplosionData data)
    {
        if (iHealth > 0) {
            iHealth--;
            Debug.Log($"Player colpito! Danni: {data.iDamage}, Vita: {iHealth}");
            if(iHealth == 0) { Debug.Log($"Player morto!"); }
        }
    }
}
