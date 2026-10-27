using Bot;
using UnityEngine;


//Simula l'input di un utente, implementando gli stessi metodi (tramite IPlayerInput)
public class BotInputHandler : MonoBehaviour, IPlayerInput
{
    private IBotStrategy _strategy;
    private BotContext _botContext;
    private Vector2 _currentMove;
    private PlayerMove _playerMove;

    [SerializeField] private GameData gameData;
    [SerializeField] private BotStrategyType strategy = BotStrategyType.Random;

    
    public void Initialize(CharacterData characterData, PlayerInstanceData instanceData)
    {
        BotInstanceData botData = instanceData as BotInstanceData;
        if (botData == null)
        {
            Debug.LogError($"[BotInputHandler] Atteso BotInstanceData, ricevuto {instanceData?.GetType().Name}", this);
            enabled = false;
            return;
        }
        if (gameData == null)
        {
            Debug.LogError("[BotInputHandler] GameData non assegnato nel prefab.", this);
            enabled = false;
            return;
        }

        //Se un movimento viene bloccato, HandleMoveBlocked può cercare un’altra mossa
        _playerMove = GetComponent<PlayerMove>();
        if (_playerMove != null) _playerMove.OnMoveBlocked += HandleMoveBlocked;

        _botContext = new BotContext
        {
            Status = BotContext.MovingStatus.Exploring,
            LastBombPosition = transform.position,
            Position = transform.position,
            CharacterData = characterData,
            InstanceData = botData,
            GameData = gameData,
            BombHandler = GetComponent<PlayerBombHandler>(),
            NetworkBombHandler = GetComponent<PhotonBombHandler>(),
            Transform = transform
        };

        _botContext.BombCooldownRemaining = botData.bombCooldown;
        _strategy = CreateStrategy(strategy);
    }

    private IBotStrategy CreateStrategy(BotStrategyType strategyType)
    {
        switch (strategyType)
        {
            case BotStrategyType.Random: return new S0RandomStrategy();
            case BotStrategyType.PlaceAndRun: return new S1PlaceAndRun();
            case BotStrategyType.Explorer: return new S2ExplorerStrategy();
            case BotStrategyType.Kamikaze: return new S3KamikazeStrategy();
            case BotStrategyType.Pacifist: return new S4PacifistStrategy();
            default:
                Debug.LogError($"Strategia bot non valida: {strategyType}. Uso Random.", this);
                return new S0RandomStrategy();
        }
    }

    /*
     * Ottiene la direzione di movimento chiamandola da DecideMove (in BotCommonStrategy)
     */
    public Vector2 GetMoveInput()
    {
        if (_botContext == null || _strategy == null) return Vector2.zero;
        _botContext.Position = transform.position;
        _strategy.Think(_botContext);
        if (!_botContext.CharacterData.isMoving)
        {
            _currentMove = _strategy.DecideMove(_botContext);
        }
        return _currentMove;
    }

    /*
     * Chiamata per controllare se è possibile (logicamente e fisicamente) raggiungere la tile di destinazione
     */
    public bool CanMoveTo(Vector3 destination)
    {
        if (_botContext == null) return false;
        float stepTime = _botContext.GameData.fCellSize / Mathf.Max(0.1f, _botContext.CharacterData.moveSpeed);
        return BotToolkit.CanReachCell(_botContext, destination, stepTime * 0.5f);
    }

    /*
     * Se il movimento viene bloccato per qualche motivo, viene chiesto alla strategia qual è il prossimo movimento
     */
    private void HandleMoveBlocked()
    {
        if (_botContext == null) return;
        _botContext.Position = transform.position;
        _currentMove = _strategy.DecideMove(_botContext);
    }

    private void OnDestroy()
    {
        if (_playerMove != null) _playerMove.OnMoveBlocked -= HandleMoveBlocked;
    }

    public bool GetBombInput()
    {
        return _botContext != null && _strategy != null && _strategy.DecideBomb(_botContext);
    }
}
