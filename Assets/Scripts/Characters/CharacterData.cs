using UnityEngine;

public abstract class CharacterData : ScriptableObject
{
    [Header("Game-stats")]
    public int maxHp;
    public int hp;
    public int maxBombs;
    public float moveSpeed;
    public float invincibilityDuration;

    [Header("Animator")]
    public E_Animator animatorType;
}
