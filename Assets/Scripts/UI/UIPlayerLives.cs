using UnityEngine;
using UnityEngine.UI;


public class UIPlayerLives : MonoBehaviour
{

    [Header("Target Player")]
    [SerializeField] private int playerID;

    [Header("Heart Images")]
    [SerializeField] private Image[] hearts;

    [Header("Heart Sprite")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    [Header("UI Fade")]
    [SerializeField] private CanvasGroup canvasGroup;

    private PlayerController targetPlayer;
    private PlayerHealth playerHealth;

    private int maxLives;
    private int currentLives;


    private void Start()
    {
        FindAndBindPlayer();
    }


    // Funzione che viene chiamata solo nell'editor, utile per debug quando cambia qualcosa nell'ispector
    private void OnValidate()
    {
        UpdateHearts(currentLives);
        SetDead(currentLives <= 0);
    }

    private void OnDestroy()
    {
        if(playerHealth != null)
        {
            playerHealth.OnHpChanged -= OnHpChangedHandler;
        }
    }

    // Cerca il playyer in scena con l'ID richiesto 
    private void FindAndBindPlayer()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        Debug.Log($"Trovati {players.Length} player in scena per UI box ID {playerID}", this);

        foreach (PlayerController player in players)
        {
            Debug.Log($"Player trovato con ID = {player.PlayerID}", player);

            if (player.PlayerID == playerID)
            {
                targetPlayer = player;
                break;
            }
        }

        if (targetPlayer == null)
        {
            Debug.LogWarning($"UIPlayerLives: No player found with ID {playerID}", this);
            return;
        }

        // Recupera la playerHealth 
        playerHealth = targetPlayer.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("PlayerHealth non trovato!", targetPlayer);
            return;
        }

        Initialize(targetPlayer, targetPlayer.MaxHealth);

        // Collegamento dell'evento alla UI
        playerHealth.OnHpChanged += OnHpChangedHandler;

    }


    public void Initialize(PlayerController player, int startLives)
    {
        targetPlayer = player;
        maxLives = player.MaxHealth;
        currentLives = Mathf.Clamp(startLives, 0, maxLives);

        UpdateHeartsVisual();
        SetDead(currentLives <= 0);
    }

    // Aggiorna le immagini dei cuori in base al numero di vite attuali
    public void UpdateHearts(int lives)
    {
        currentLives = Mathf.Clamp(lives, 0, maxLives);
        UpdateHeartsVisual();
        SetDead(currentLives <= 0);
    }


    private void OnHpChangedHandler(int currentHp, int maxHp)
    {
        maxLives = maxHp;

        UpdateHearts(currentHp);
    }


    private void UpdateHeartsVisual()
    {
        for (int i = 0; i < hearts.Length; i++) {

            if (hearts[i] == null) continue;

            hearts[i].sprite = i < currentLives ? fullHeart : emptyHeart;
            hearts[i].enabled = i < maxLives;
        
        
        }
    }

    // Se il player muore rende i cuori più trasparenti
    public void SetDead(bool isDead)
    {
        if (canvasGroup == null) return;
        
        canvasGroup.alpha = isDead ? 0.3f : 1f;
    }


    public PlayerController GetTargetPlayer() { return targetPlayer; }


}
