using UnityEngine;


public class BotInputHandler : MonoBehaviour, IPlayerInput
{
    private IBotStrategy _strategy;
    private BotContext _botContext;

    private Vector2 _currentMove;
    private float _moveTimer;
    private float _bombTimer;
    private bool _bombThisFrame;

    public void Initialize(CharacterData characterData, PlayerInstanceData instanceData)
    {
        BotInstanceData _instanceData = instanceData as BotInstanceData;

        if (_instanceData == null)
        {
            Debug.LogError($"[BotInputHandler] Initialize fallito: atteso BotInstanceData, ricevuto {instanceData?.GetType().Name}", this);
            return;
        }

        _botContext = new BotContext
        {    
            status = BotContext.STATUS.EXPLORING,
            LastBombPosition = transform.position,
            LastPosition = transform.position,
            Position = transform.position,
            CharacterData = characterData,
            InstanceData = _instanceData
        };

        switch (_instanceData.difficulty)
        {
            case 0:
                _strategy = new S0RandomStrategy();
                break;

            case 1:
                _strategy = new S1PlaceAndRun();
                break;

            default:
                _strategy = new S0RandomStrategy();
                break;
        }
        //Debug.Log($"[{characterData.name}] Initialized, with difficulty = {_strategy.ToString()} and status {_botContext.status}");
    }

    void Update()
    {
        _botContext.LastPosition = _botContext.Position;
        _botContext.Position = transform.position;

        if (_botContext.LastBombTimer > 0f)
        {
            //Decrementa timer, se "scade", si torna in exploring
            _botContext.LastBombTimer -= Time.deltaTime;
            if (_botContext.LastBombTimer <= 0f)
            {
                _botContext.status = BotContext.STATUS.EXPLORING;
                //Debug.Log($"[{_botContext.CharacterData.name}] status changed to {_botContext.status}");
            }
        }

        //Strategia di movimento
        _moveTimer += Time.deltaTime;
        if (!_botContext.CharacterData.isMoving && _moveTimer >= _botContext.InstanceData.moveChangeCooldown)
        {
            _moveTimer = 0f;
            _currentMove = _strategy.DecideMovement(_botContext);
            _botContext.LastDirection = _currentMove;
            //_botContext.isMoving = true;
        }

        //Strategie di piazzamento della bomba
        _bombTimer += Time.deltaTime;
        if (_bombTimer >= _botContext.InstanceData.bombCooldown)
        {
            _bombThisFrame = _strategy.DecideBomb(_botContext);
            if (_bombThisFrame) { 
                _bombTimer = 0f;
                _botContext.LastBombPosition = _botContext.Position;
                _botContext.LastBombTimer = _botContext.CharacterData.explosionData.fTimeToExplode;
            }
        }

        
    }

    public Vector2 GetMoveInput() => _currentMove;

    public bool GetBombInput()
    {
        bool val = _bombThisFrame;
        _bombThisFrame = false;   // reset: simula WasPressedThisFrame
        return val;
    }
}
