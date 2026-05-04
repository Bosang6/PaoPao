using UnityEngine;

public abstract class CharacterData : ScriptableObject
{
    [Header("Game-stats")]
    public int maxHp;
    public int hp;
    public int maxBombs;
    public float moveSpeed;
    public float invincibilityDuration;

    [Header("TODO:Sprite")]
    public Sprite playerSprite;

    [Header("TODO:Animator")]
    public E_Animator animatorType;
}
