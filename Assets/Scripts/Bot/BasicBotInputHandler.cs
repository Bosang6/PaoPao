using UnityEngine;

public class BasicBotInputHandler : MonoBehaviour, IPlayerInput
{
    private CharacterData _characterData;
    private BasicBotInstanceData _instanceData;
    
    private Vector2 _currentMove = Vector2.zero;
    private float _moveTimer = 0f;
    private float _bombTimer = 0f;
    private bool _bombThisFrame = false;
    private Vector2 _lastDirection = Vector2.zero;

    private static readonly Vector2[] Directions =
    {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right, Vector2.zero
    };

    public void Initialize(CharacterData characterData, PlayerInstanceData instanceData)
    {
        _characterData = characterData;
        _instanceData = instanceData as BasicBotInstanceData;

        if (_instanceData == null)
        {
            Debug.LogError($"[BasicBotInputHandler] Initialize fallito: atteso BasicBotInstanceData, ricevuto {instanceData?.GetType().Name}", this);
            return;
        }
    }

    public Vector2 GetMoveInput() => _currentMove;

    public bool GetBombInput()
    {
        bool val = _bombThisFrame;
        _bombThisFrame = false;   // reset: simula WasPressedThisFrame
        return val;
    }

    void Update()
    {
        UpdateMovement();
        UpdateBomb();
    }

    private void UpdateMovement()
    {
        _moveTimer += Time.deltaTime;
        if (_moveTimer >= _instanceData.moveChangeCooldown)
        {
            _moveTimer = 0f;

            //Aumenta la probabilità di proseguire dove stava andando
            if(Random.Range(1, 3) != 1)
            {
                _currentMove = _lastDirection;
            }
            else
            {
                _currentMove = Directions[Random.Range(0, Directions.Length)];
                if (_currentMove != Vector2.zero) { _lastDirection = _currentMove; }
            }
        }
    }

    private void UpdateBomb()
    {
        _bombTimer += Time.deltaTime;
        if (_bombTimer >= _instanceData.bombCooldown)
        {
            _bombTimer = 0f;
            _bombThisFrame = true;
        }
    }
}
