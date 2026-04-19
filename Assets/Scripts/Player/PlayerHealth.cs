using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private PlayerData pData;

    public void Initialize(PlayerData playerData) { 
        pData = playerData;
        pData.hp = pData.maxHp;
    }

    public int Hitted(ExplosionData data)
    {
        if (pData.hp > 0)
        {
            pData.hp--;
            Debug.Log($"Player colpito! Danni: {data.iDamage}, Vita: {pData.hp}");
            if (pData.hp == 0) { Debug.Log($"Player morto!"); }
        }
        return pData.hp;
    }

}
