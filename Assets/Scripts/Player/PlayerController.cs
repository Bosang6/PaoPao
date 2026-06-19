using UnityEngine;

public class PlayerController : MonoBehaviour, IExplosionReceiver
{
    private float invicibilityTimer = 0f;

    [Header("Game Settings")]
    [SerializeField] private GameData _gameData;

    [Header("Player Settings")]
    [SerializeField] private CharacterData _characterData;
    [SerializeField] private PlayerInstanceData _instanceData;


    //Elenco componenti controllati da PlayerController
    private IPlayerInput _pInput;
    private PlayerMove _pMove;
    private PlayerBombHandler _pBombHandler;
    private PlayerHealth _pHealth;

    public int MaxHealth => _characterData.maxHp;
    public int PlayerID => _instanceData.playerID;

    public bool IsHuman => _instanceData.isHuman;

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
        _pInput.Initialize(_characterData, _instanceData);
        _pMove.Initialize(_gameData, _characterData, _instanceData);
        _pBombHandler.Initialize(_gameData, _characterData);
        _pHealth.Initialize(_characterData);
    }

    public void Update()
    {
        _pMove.HandleInput(_pInput.GetMoveInput());
        if (_pInput.GetBombInput()) { _pBombHandler.TryPlaceBomb(); }
        if(invicibilityTimer > 0) { invicibilityTimer -= Time.deltaTime; }  //Se in fase invincibile
    }

    public void OnHitByExplosion(ExplosionData data)
    {
        if (invicibilityTimer > 0) { return; }  //Ancora in fase di invincibilit�
        //_pHealth.Hitted(data) restituisce il numero di hp rimanenti
        if (_pHealth.Hitted(data) > 0)
        {
            //_pMove.Respawn();
            _pMove.SetAnimatorHurtingTrigger();
        }
        else
        {
            _pMove.SetAnimatorIsDead();
            Death();
        }
        invicibilityTimer = _characterData.invincibilityDuration;
        //_pHealth?.StartBlink(playerData.invincibilityDuration); //Lampeggia durante l'invincibilit�
    }

    private void Death()
    {
        OnPlayerDied?.Invoke(this); //This per indicare quale player � morto
        Destroy(gameObject, 0.5f);
    }
}
