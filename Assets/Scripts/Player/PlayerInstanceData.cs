using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInstanceData : ScriptableObject
{
    public enum E_PlayerSlotType { LocalHuman, AI, NetworkHuman }

    [Header("Identity")]
    public E_PlayerSlotType type;
    public string playerName;
    public int playerID;

    [Header("Spawn")]
    public Vector3 spawnPosition;
    public Quaternion spawnRotation;

}
