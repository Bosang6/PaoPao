using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerData", menuName = "PaoPaoOBJ/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Identity")]
    public string playerName;
    public int playerID;
    public bool isHuman;
    public Color playerColor;
    public Sprite playerSprite;

    [Header("Input")]
    public InputActionAsset inputActionAsset;
    public string controlScheme;

    [Header("Game-stats")]
    public int maxHp;
    public int hp;
    public int maxBombs;
    public float moveSpeed;
    public Vector3 spawnPosition;
    public Quaternion spawnRotation;

    [Header("Collision")]
    public LayerMask lmCollisionLayer;
}
