using UnityEngine;

public interface IPlayerInput
{
    void Initialize(PlayerData data);
    Vector2 GetMoveInput();
    bool GetBombInput();   
}
