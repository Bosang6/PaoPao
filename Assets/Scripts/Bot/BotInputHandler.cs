using Bot;
using UnityEngine;


//Simula l'input di un utente, implementando gli stessi metodi (tramite IPlayerInput)
public class BotInputHandler : MonoBehaviour, IPlayerInput
{
    [Header("Bot Configuration")]
    [SerializeField] private GameData gameData;
    [SerializeField] private BotStrategyType strategyType = BotStrategyType.Random;
    [SerializeField, Min(0.01f)] private float idleDecisionRetryInterval = 0.25f;

    private PlayerMove playerMove;          //Componente che muove il personaggio
    private Vector2 currentMove;            //Ultima direzione scelta (usata durante il movimento)
    private BotContext botContext;          //Informazioni conosciute dal bot
    private IBotStrategy activeStrategy;    //Componente di ia del bot, varia in base al tipo di strategia
    private Vector2Int lastThoughtCell;     //Tile dove il bot ha preso l'ultima decisione
    private bool hasThoughtCell;            //Flag che indica se in questa tile ha già preso una decisione 
    private float nextIdleDecisionTime;     //Timer che indica quanti secondi mancano alla prossima decisione

    public void Initialize(CharacterData characterData, PlayerInstanceData instanceData)
    {
        idleDecisionRetryInterval = Mathf.Max(0.01f, idleDecisionRetryInterval);

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
        playerMove = GetComponent<PlayerMove>();
        if (playerMove != null) { playerMove.OnMoveBlocked += HandleMoveBlocked; }

        botContext = new BotContext
        {
            Status = BotContext.MovingStatus.Exploring,
            LastBombPosition = transform.position,
            Position = transform.position,
            CharacterData = characterData,
            InstanceData = botData,
            GameData = gameData,
            BombHandler = GetComponent<PlayerBombHandler>(),
            NetworkBombHandler = GetComponent<PhotonBombHandler>(),
            Transform = transform,
            BombCooldownRemaining = botData.bombCooldown
        };

        activeStrategy = CreateStrategy(strategyType);
    }

    private IBotStrategy CreateStrategy(BotStrategyType newStrategyType)
    {
        switch (newStrategyType)
        {
            case BotStrategyType.Random: return new S0RandomStrategy();
            case BotStrategyType.PlaceAndRun: return new S1PlaceAndRun();
            case BotStrategyType.Explorer: return new S2ExplorerStrategy();
            case BotStrategyType.Kamikaze: return new S3KamikazeStrategy();
            case BotStrategyType.Pacifist: return new S4PacifistStrategy();
            default:
                Debug.LogError($"Strategia bot non valida: {newStrategyType}. Uso Random.", this);
                return new S0RandomStrategy();
        }
    }

    /*
     * Ottiene la direzione di movimento chiamandola da DecideMove (in BotCommonStrategy)
     */
    public Vector2 GetMoveInput()
    {
        if (botContext == null || activeStrategy == null) return Vector2.zero;

        //Aggiorna costantemente i timer
        botContext.Position = transform.position;
        activeStrategy.UpdateTimers(botContext, Time.deltaTime);

        //Se si è in movimento, lo si continua
        if (botContext.CharacterData.isMoving) return currentMove;

        Vector3 cellPosition = botContext.CellPosition;
        Vector2Int currentCell = new Vector2Int(
            Mathf.RoundToInt(cellPosition.x / gameData.fCellSize),
            Mathf.RoundToInt(cellPosition.y / gameData.fCellSize));

        bool reachedNewCell = !hasThoughtCell || currentCell != lastThoughtCell;
        if (reachedNewCell)
        {
            lastThoughtCell = currentCell;
            hasThoughtCell = true;
        }

        //Se è fermo senza una mossa, riprova a castarne una ogni [nextIdleDecisionTime] secondi
        nextIdleDecisionTime -= Time.deltaTime;
        bool retryWhileIdle = currentMove == Vector2.zero && nextIdleDecisionTime <= 0f;
        if (reachedNewCell || retryWhileIdle)
        {
            activeStrategy.Think(botContext);
            currentMove = activeStrategy.DecideMove(botContext);
            nextIdleDecisionTime = idleDecisionRetryInterval;
        }

        return currentMove;
    }

    public bool GetBombInput()
    {
        return botContext != null && activeStrategy != null && activeStrategy.DecideBomb(botContext);
    }

    /*
     * Chiamata per controllare se è possibile (logicamente e fisicamente) raggiungere la tile di destinazione
     */
    public bool CanMoveTo(Vector3 destination)
    {
        if (botContext == null) return false;
        float stepTime = botContext.GameData.fCellSize / Mathf.Max(0.1f, botContext.CharacterData.moveSpeed);
        return BotToolkit.CanReachCell(botContext, destination, stepTime * 0.5f);
    }

    /*
     * Se il movimento viene bloccato per qualche motivo, viene chiesto alla strategia qual è il prossimo movimento
     */
    private void HandleMoveBlocked()
    {
        if (botContext == null) return;
        botContext.Position = transform.position;
        currentMove = activeStrategy.DecideMove(botContext);
    }

    private void OnDestroy()
    {
        if (playerMove != null) playerMove.OnMoveBlocked -= HandleMoveBlocked;
    }
}
