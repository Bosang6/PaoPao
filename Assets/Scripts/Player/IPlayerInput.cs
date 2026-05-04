using UnityEngine;

public interface IPlayerInput
{
    void Initialize(CharacterData characterData, PlayerInstanceData instanceData);
    Vector2 GetMoveInput();
    bool GetBombInput();   
}
