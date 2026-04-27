using UnityEngine;

public class PlayerController : MonoBehaviour, IExplosionReceiver
{
    private float invicibilityTimer = 0f;

    [Header("Player Settings")]
    [SerializeField] private PlayerData playerData;

    [Header("Game Settings")]
    [SerializeField] private GameData gameData;

    //Elenco componenti controllati da PlayerController
    private IPlayerInput _pInput;
    private PlayerMove _pMove;
    private PlayerBombHandler _pBombHandler;
    private PlayerHealth _pHealth;

    public int MaxHealth => playerData.maxHp;
    public int PlayerID => playerData.playerID;

    public bool IsHuman => playerData.isHuman;

    //Evento da invocare alla morte 
    public event System.Action<PlayerController> OnPlayerDied;

    private void Awake()
    {
        //Recupera i componenti
        _pInput = GetComponent<IPlayerInput>();
        _pMove = GetComponent<PlayerMove>();
        _pBombHandler = GetComponent<PlayerBombHandler>();
        _pHealth = GetComponent<PlayerHealth>();

        //Inizializza i componenti
        _pInput.Initialize(playerData);
        _pMove.Initialize(playerData, gameData);
        _pBombHandler.Initialize(playerData, gameData);
        _pHealth.Initialize(playerData);
    }

    public void Update()
    {
        _pMove.HandleInput(_pInput.GetMoveInput());
        if (_pInput.GetBombInput()) { _pBombHandler.TryPlaceBomb(); }
        if(invicibilityTimer > 0) { invicibilityTimer -= Time.deltaTime; }  //Se in fase invincibile
    }

    public void OnHitByExplosion(ExplosionData data)
    {
        if (invicibilityTimer > 0) { return; }  //Ancora in fase di invincibilità
        //_pHealth.Hitted(data) restituisce il numero di hp rimanenti
        if (_pHealth.Hitted(data) > 0) { _pMove.Respawn(); } else { Death(); }
        invicibilityTimer = playerData.invincibilityDuration;
        _pHealth?.StartBlink(playerData.invincibilityDuration); //Lampeggia durante l'invincibilità
    }

    private void Death()
    {
        OnPlayerDied?.Invoke(this); //This per indicare quale player è morto
        Destroy(gameObject);
    }
}
