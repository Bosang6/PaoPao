using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "PaoPaoOBJ/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Identity")]
    public string playerName;
    public Color playerColor;
    public Sprite playerSprite;

    [Header("Input")]
    public string horizontalAxis;
    public string verticalAxis;
    public KeyCode bombKey;

    [Header("Stats")]
    public int maxBombs;
    public float moveSpeed;

    [Header("Collision")]
    public LayerMask lmCollisionLayer;
}
