using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private PlayerData pData;
    private int currentHp;

    // Getter pub per leggere gli hp correnti dall'esterno
    public int CurrentHp => currentHp;
    // Getter pub per leggere gli hp massimi dall'esterno
    public int MaxHp => pData != null ? pData.maxHp : 0;


    /* 
     * Evento che viene chiamato ogni volta che gli hp cambiano.
     * Serve per aggiornare automaticamente la UI senza fare controlli ogni frame.
     */
    public event Action<int,int> OnHpChanged;


    public void Initialize(PlayerData playerData) { 
        pData = playerData;
        currentHp = pData.maxHp;
        OnHpChanged?.Invoke(currentHp, MaxHp);
    }

    public int Hitted(ExplosionData data)
    {

        if (currentHp <= 0) return currentHp;

        currentHp -= data.iDamage;
        currentHp = Mathf.Clamp(currentHp, 0, MaxHp);
        Debug.Log($"Player colpito! Danni: {data.iDamage}, Vita: {currentHp}");

        OnHpChanged?.Invoke(currentHp, MaxHp);

        if(currentHp == 0) { Debug.Log($"Player morto!"); }

        return currentHp;
    }

}
