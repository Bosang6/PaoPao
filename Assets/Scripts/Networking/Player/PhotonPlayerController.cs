using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;


/*
 * Gestisce un personaggio nella partita multiplayer.
 *
 * Legge i dati ricevuti dallo spawn Photon, crea i dati runtime
 * e configura input, movimento, salute, bombe e audio.
 * Il danno e la morte vengono determinati dal Master Client.
 */

[RequireComponent(typeof(PhotonView))]
public sealed class PhotonPlayerController :  MonoBehaviourPun, IPunInstantiateMagicCallback
{
    [Header("Game Settings")]
    [SerializeField] private GameData gameData;

    [Header("Character Template")]
    [Tooltip("CharacterData corrispondente al personaggio del prefab. Verrà clonata durante il runtime.")]
    [SerializeField] private CharacterData characterDataTemplate;

    [Header("Instance Data Templates")]
    [Tooltip( "Dati utilizzati quando il personaggio è controllato da un giocatore umano.")]
    [SerializeField] private HumanInstanceData humanInstanceDataTemplate;
    [Tooltip("Dati utilizzati quando il personaggio è controllato dalla AI.")]
    [SerializeField] private BotInstanceData botInstanceDataTemplate;


    private LocalInputHandler localInputHandler;
    private BotInputHandler botInputHandler;

    private PlayerMove playerMove;
    private PlayerBombHandler playerBombHandler;
    private PlayerHealth playerHealth;
    private PlayerAudio playerAudio;

    private CharacterData runtimeCharacterData;
    private PlayerInstanceData runtimeInstanceData;

    private IPlayerInput activeInput;

    private PhotonBombHandler photonBombHandler;

    private bool isInitialized;
    private bool isAI;
    private int slotIndex = -1;

    // L'invincibilità viene controllata dal Master Client, che possiede l'autorità sul danno
    private float invincibilityTimer;
    private bool isDead;

    public int SlotIndex => slotIndex;

    public bool IsAI => isAI;

    public bool IsLocalHuman => !isAI && photonView.IsMine;

    public bool HasSimulationAuthority => photonView.IsMine;

    public int MaxHealth => runtimeCharacterData != null ? runtimeCharacterData.maxHp : 0;

    public PlayerHealth Health => playerHealth;

    private bool isMatchEnded;

    private Coroutine destroyAfterDeathCoroutine;

    public int ViewId => photonView.ViewID;

    // Evento autoritativo invocato esclusivamente sul Master quando il personaggio muore
    public event Action<PhotonPlayerController, int> OnNetworkPlayerDied;



    // Recupera tutti i componenti necessari alla gestione del personaggio multiplayer e delle bombe sincronizzate
    private void Awake()
    {
        localInputHandler = GetComponent<LocalInputHandler>();

        botInputHandler = GetComponent<BotInputHandler>();

        playerMove = GetComponent<PlayerMove>();

        playerBombHandler = GetComponent<PlayerBombHandler>();

        playerHealth = GetComponent<PlayerHealth>();

        playerAudio = GetComponent<PlayerAudio>();

        photonBombHandler = GetComponent<PhotonBombHandler>();
    }


    // Photon richiama questo metodo dopo aver creato il prefab
    // InstantiationData:
    // 0 = slotIndex
    // 1 = isAI
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = photonView.InstantiationData;

        if (instantiationData == null || instantiationData.Length < 2)
        {
            Debug.LogError("[PhotonPlayerController] " + "InstantiationData mancanti o incomplete.", this);

            DisableInputHandlers();
            return;
        }

        try
        {
            slotIndex = Convert.ToInt32(instantiationData[0]);

            isAI = Convert.ToBoolean(instantiationData[1]);
        }
        catch (Exception exception)
        {
            Debug.LogError("[PhotonPlayerController] " + "Impossibile leggere InstantiationData. " + $"Errore: {exception.Message}", this);

            DisableInputHandlers();
            return;
        }

        InitializeRuntimePlayer();
    }


    private void InitializeRuntimePlayer()
    {
        if (!ValidateReferences())
        {
            DisableInputHandlers();
            return;
        }

        // CharacterData contiene anche isMoving. Creiamo una copia per evitare che 
        // più personaggi condividano lo stesso runtime
        runtimeCharacterData = Instantiate(characterDataTemplate);

        runtimeCharacterData.isMoving = false;

        runtimeInstanceData = CreateRuntimeInstanceData();

        if (runtimeInstanceData == null)
        {
            DisableInputHandlers();
            return;
        }

        runtimeInstanceData.playerID = slotIndex;

        runtimeInstanceData.spawnPosition = transform.position;

        runtimeInstanceData.spawnRotation = transform.rotation;

        playerMove.Initialize(gameData, runtimeCharacterData, runtimeInstanceData);

        playerBombHandler.Initialize(gameData, runtimeCharacterData, IsLocalHuman);

        playerHealth.Initialize(runtimeCharacterData);

        photonBombHandler.Initialize(gameData, runtimeCharacterData);

        ConfigureInput();

        ConfigureAudio();

        isInitialized = true;

        if (PhotonGameManager.Instance != null)
        {
            PhotonGameManager.Instance.RegisterPlayer(this);
        }
        else
        {
            Debug.LogError("[PhotonPlayerController] PhotonGameManager non presente nella scena.", this);
        }

    }


    private PlayerInstanceData CreateRuntimeInstanceData()
    {
        if (isAI)
        {
            if (botInstanceDataTemplate == null)
            {
                Debug.LogError("[PhotonPlayerController] " + "BotInstanceData Template non assegnato.", this);

                return null;
            }

            BotInstanceData runtimeBotData = Instantiate(botInstanceDataTemplate);

            runtimeBotData.type = PlayerInstanceData.E_PlayerSlotType.AI;

            return runtimeBotData;
        }

        if (humanInstanceDataTemplate == null)
        {
            Debug.LogError("[PhotonPlayerController] " + "HumanInstanceData Template non assegnato.", this);

            return null;
        }

        HumanInstanceData runtimeHumanData = Instantiate(humanInstanceDataTemplate);

        runtimeHumanData.type = photonView.IsMine ? PlayerInstanceData.E_PlayerSlotType.LocalHuman : PlayerInstanceData.E_PlayerSlotType.NetworkHuman;

        return runtimeHumanData;
    }


    private void ConfigureInput()
    {
        DisableInputHandlers();

        if (isAI)
        {
            if (botInputHandler == null)
            {
                Debug.LogError("[PhotonPlayerController] " + "BotInputHandler non trovato.", this);
                return;
            }

            botInputHandler.enabled = true;

            botInputHandler.Initialize(runtimeCharacterData, runtimeInstanceData);

            activeInput = botInputHandler;
            return;
        }

        if (localInputHandler == null)
        {
            Debug.LogError("[PhotonPlayerController] " + "LocalInputHandler non trovato.", this);

            return;
        }

        localInputHandler.enabled = true;

        localInputHandler.Initialize(runtimeCharacterData, runtimeInstanceData);

        activeInput = localInputHandler;
    }


    private void DisableInputHandlers()
    {
        activeInput = null;

        if (localInputHandler != null)
        {
            localInputHandler.enabled = false;
        }

        if (botInputHandler != null)
        {
            botInputHandler.enabled = false;
        }
    }


    private void ConfigureAudio()
    {
        if (playerAudio == null)
        {
            return;
        }

        if (IsLocalHuman)
        {
            playerAudio.Initialize(runtimeCharacterData, playerBombHandler);

            return;
        }

        playerAudio.Initialize(runtimeCharacterData);
    }


    private void Update()
    {
        // Il Master aggiorna il timer di invincibilità per tutte le copie autoritative dei personaggi
        if (PhotonNetwork.IsMasterClient && invincibilityTimer > 0f)
        {
            invincibilityTimer -= Time.deltaTime;
        }

        if (!isInitialized || isDead || isMatchEnded || !photonView.IsMine || activeInput == null)
        {
            return;
        }

        playerMove.HandleInput(activeInput.GetMoveInput());

        bool bombRequested = activeInput.GetBombInput();

        if (bombRequested)
        {
            photonBombHandler.TryRequestBomb();
        }
    }

    // Viene chiamato dal PhotonExplosionManager esclusivamente sul Master Client quando una fiamma raggiunge il personaggio
    public void TryApplyExplosionDamage( ExplosionData explosionData, int attackerPlayerViewId)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (!isInitialized || isDead || explosionData == null)
        {
            return;
        }

        if (invincibilityTimer > 0f)
        {
            return;
        }

        if (playerHealth.CurrentHp <= 0)
        {
            return;
        }

        int remainingHp = playerHealth.Hitted(explosionData);

        invincibilityTimer = runtimeCharacterData.invincibilityDuration;

        bool died = remainingHp <= 0;

        ApplyExplosionHitFeedback(died);

        photonView.RPC(
            nameof(RPC_ApplyExplosionHitResult),
            RpcTarget.Others,
            remainingHp,
            died
        );

        if (died)
        {
            // Viene eseguito esclusivametne sul Master, quindi anche l'evento di morte è authoritative
            OnNetworkPlayerDied?.Invoke(this, attackerPlayerViewId);
            ScheduleOwnedPlayerDestruction();
        }
    }


    [PunRPC]
    private void RPC_ApplyExplosionHitResult(int remainingHp, bool died)
    {
        if (!isInitialized)
        {
            Debug.LogError("[PhotonPlayerController] " + "Risultato del danno ricevuto prima dell'inizializzazione.", this);
            return;
        }

        playerHealth.SetCurrentHp(remainingHp);

        ApplyExplosionHitFeedback(died);

        if (died)
        {
            ScheduleOwnedPlayerDestruction();
        }

    }

    // Riproduce localmente la  stessa reazione grafica utilizzata dal PlayerController single Player
    private void ApplyExplosionHitFeedback(bool died)
    {
        if (died)
        {
            isDead = true;

            DisableInputHandlers();

            playerMove.SetAnimatorIsDead();
            playerAudio?.PlayDeath();

            return;
        }

        playerMove.SetAnimatorHurtingTrigger();
    }



    private bool ValidateReferences()
    {
        if (gameData == null)
        {
            Debug.LogError("[PhotonPlayerController] " + "GameData non assegnato.", this);
            return false;
        }

        if (characterDataTemplate == null)
        {
            Debug.LogError("[PhotonPlayerController] " + "CharacterData Template non assegnato.", this);
            return false;
        }

        if (playerMove == null)
        {
            Debug.LogError("[PhotonPlayerController] " + "PlayerMove non trovato.", this);
            return false;
        }

        if (playerBombHandler == null)
        {
            Debug.LogError("[PhotonPlayerController] PlayerBombHandler non trovato.", this);
            return false;
        }

        if (playerHealth == null)
        {
            Debug.LogError("[PhotonPlayerController] PlayerHealth non trovato.", this);
            return false;
        }

        if (photonBombHandler == null)
        {
            Debug.LogError("[PhotonPlayerController] PhotonBombHandler non trovato.", this);
            return false;
        }

        return true;
    }

    // Avvia la distruzione soltanto sul client che possiede realmente questo PhotonView
    private void ScheduleOwnedPlayerDestruction()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (destroyAfterDeathCoroutine != null)
        {
            return;
        }

        destroyAfterDeathCoroutine = StartCoroutine(DestroyOwnedPlayerAfterDeath());
    }


    // Lascia trascorrere il tempo necessario all'animazione di morte, poi distrugge il personaggio su tutta la rete.
    private IEnumerator DestroyOwnedPlayerAfterDeath()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        destroyAfterDeathCoroutine = null;

        if (!PhotonNetwork.InRoom)
        {
            yield break;
        }

        if (!photonView.IsMine)
        {
            yield break;
        }

        PhotonNetwork.Destroy(gameObject);
    }


    // Disabilita definitivamente il controllo del personaggio quando il Master dichiara conclusa la partita
    public void SetMatchEnded()
    {
        isMatchEnded = true;

        DisableInputHandlers();
    }


    private void OnDestroy()
    {
        if (runtimeCharacterData != null)
        {
            Destroy(runtimeCharacterData);
        }

        if (runtimeInstanceData != null)
        {
            Destroy(runtimeInstanceData);
        }

        if (PhotonGameManager.Instance != null)
        {
            PhotonGameManager.Instance.UnregisterPlayer(this);
        }

    }

}