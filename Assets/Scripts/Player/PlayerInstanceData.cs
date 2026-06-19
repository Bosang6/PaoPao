using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInstanceData : ScriptableObject
{
    [Header("Identity")]
    public string playerName;
    public int playerID;
    public Color playerColor;

    [Header("Spawn")]
    public Vector3 spawnPosition;
    public Quaternion spawnRotation;

    [Header("TODO:isHuman")]
    public bool isHuman;
}
