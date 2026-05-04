using System;
using UnityEngine;
using System.Collections;


public class PlayerHealth : MonoBehaviour
{
    private CharacterData _characterData;
    private int currentHp;
    private float blinkInterval = 0.3f; // Velocità del blink
    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    // Getter pub per leggere gli hp correnti dall'esterno
    public int CurrentHp => currentHp;
    // Getter pub per leggere gli hp massimi dall'esterno
    public int MaxHp => _characterData != null ? _characterData.maxHp : 0;


    /* 
     * Evento che viene chiamato ogni volta che gli hp cambiano.
     * Serve per aggiornare automaticamente la UI senza fare controlli ogni frame.
     */
    public event Action<int,int> OnHpChanged;


    public void Initialize(CharacterData characterData) { 
        _characterData = characterData;
        currentHp = _characterData.maxHp;
        OnHpChanged?.Invoke(currentHp, MaxHp);
        spriteRenderer = GetComponent<SpriteRenderer>();
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


    //IA, il player dovrebbe lampeggiare mentre è invincibile, da testare dopo aver impostato l'animator
    public void StartBlink(float duration)
    {
        if (blinkCoroutine != null) { StopCoroutine(blinkCoroutine); }
        blinkCoroutine = StartCoroutine(BlinkRoutine(duration));
    }

    private IEnumerator BlinkRoutine(float duration)
    {
        float elapsed = 0f;
        bool visible = true;
        Color normal = spriteRenderer.color;
        Color transparent = new Color(normal.r, normal.g, normal.b, 0.2f);

        while (elapsed < duration)
        {
            spriteRenderer.color = visible ? transparent : normal;
            visible = !visible;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        spriteRenderer.color = normal; // Ripristina colore originale
        blinkCoroutine = null;
    }

}
