using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "PaoPaoOBJ/CharacterData")]
public class CharacterData : ScriptableObject
{
    public enum E_Character
    {
        Bomberman,
        Penguin,
        Slime1,
        Slime2,
        Slime3
    }


    [Header("Game-stats")]
    public E_Character type;
    public int maxHp;
    public int hp;
    public int maxBombs;
    public float moveSpeed;
    public float invincibilityDuration;
    public bool isMoving = false;

    [Header("ExplosionData")]
    public ExplosionData explosionData;

    [Header("Animator")]
    public E_Animator animatorType;
}
