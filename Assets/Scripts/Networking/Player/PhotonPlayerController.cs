using System;
using Photon.Pun;
using UnityEngine;


/*
 * Controlla un personaggio nella partita multiplayer
 *
 * È utilizzato esclusivamente dai prefab presenti in Resources/PhotonPrefabs.
 *
 * Responsabilità:
 * - leggere i dati ricevuti durante PhotonNetwork.Instantiate;
 * - creare copie runtime di CharacterData e PlayerInstanceData;
 * - attivare l'input locale soltanto sul proprietario;
 * - attivare l'intelligenza artificiale soltanto sul Master;
 * - impedire alle copie remote di eseguire input;
 * - inizializzare movimento, salute, bombe e audio.
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


    [Header("Temporary Networking Settings")]
    [Tooltip( "Le bombe restano disattivate finché non implementiamo " +
        "lo spawn sincronizzato delle bombe."
    )]
    [SerializeField] private bool enableBombPlacement;


    private LocalInputHandler localInputHandler;
    private BotInputHandler botInputHandler;

    private PlayerMove playerMove;
    private PlayerBombHandler playerBombHandler;
    private PlayerHealth playerHealth;
    private PlayerAudio playerAudio;

    private CharacterData runtimeCharacterData;
    private PlayerInstanceData runtimeInstanceData;

    private IPlayerInput activeInput;

    private bool isInitialized;
    private bool isAI;
    private int slotIndex = -1;


    public int SlotIndex => slotIndex;

    public bool IsAI => isAI;

    public bool IsLocalHuman => !isAI && photonView.IsMine;

    public bool HasSimulationAuthority => photonView.IsMine;

    public int MaxHealth => runtimeCharacterData != null ? runtimeCharacterData.maxHp : 0;

    public PlayerHealth Health => playerHealth;


    private void Awake()
    {
        localInputHandler = GetComponent<LocalInputHandler>();

        botInputHandler = GetComponent<BotInputHandler>();

        playerMove = GetComponent<PlayerMove>();

        playerBombHandler = GetComponent<PlayerBombHandler>();

        playerHealth = GetComponent<PlayerHealth>();

        playerAudio = GetComponent<PlayerAudio>();
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

        ConfigureInput();

        ConfigureAudio();

        isInitialized = true;

        Debug.Log(
            "[PhotonPlayerController] " +
            $"Actor proprietario: {photonView.OwnerActorNr}. " +
            $"Slot: {slotIndex}. " +
            $"Character: {runtimeCharacterData.type}. " +
            $"Type: {runtimeInstanceData.type}. " +
            $"IsMine: {photonView.IsMine}."
        );
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

        // Solo il proprietario Photon simula il personaggio.
        if (!photonView.IsMine)
        {
            activeInput = null;
            return;
        }

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
        if (!isInitialized || !photonView.IsMine || activeInput == null)
        {
            return;
        }

        playerMove.HandleInput( activeInput.GetMoveInput());

        bool bombRequested = activeInput.GetBombInput();

        if (enableBombPlacement && bombRequested)
        {
            playerBombHandler.TryPlaceBomb();
        }
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
            Debug.LogError("[PhotonPlayerController] " + "PlayerBombHandler non trovato.", this);

            return false;
        }

        if (playerHealth == null)
        {
            Debug.LogError("[PhotonPlayerController] " + "PlayerHealth non trovato.", this);

            return false;
        }

        return true;
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
    }
}