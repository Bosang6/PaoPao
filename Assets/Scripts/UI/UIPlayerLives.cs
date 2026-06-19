using UnityEngine;
using UnityEngine.UI;


/* 
 * Questo script gestisce l'HUD delle vite del player, mostrando cuori pieni o vuoti in base alla salute attuale
*/

public class UIPlayerLives : MonoBehaviour
{

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
        // Stato iniziale dell'HUD, in attesa di trovare il player da seguire
        SetDead(false);
    }

    private void OnDestroy()
    {
        Unbind();
    }


    public void Bind(PlayerController player)
    {
        if(player == null)
        {
            Debug.LogWarning("UIPlayerLives: impossibile fare Bind, player nullo.", this);
            return;
        }

        // Se questo HUD era già collegato ad un player, prima rimuove il vecchio collegamento
        Unbind();

        targetPlayer = player;
        playerHealth = targetPlayer.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError("UIPlayerLives: PlayerHealth non trovato sul player.", targetPlayer);
            return;
        }

        maxLives = targetPlayer.MaxHealth;
        currentLives = maxLives;

        UpdateHeartsVisual();
        SetDead(false);

        // Da questo momento, ogni volta che il player perde vita viene aggiornata l'HUD
        playerHealth.OnHpChanged += OnHpChangedHandler;
    }

    // Scollega questo HUD dal player attuale. Serve per eveitare eventi pendenti o riferimenti vecchi
    public void Unbind()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHpChanged -= OnHpChangedHandler;
        }

        targetPlayer = null;
        playerHealth = null;
    }


    // Handler per l'evento di cambio HP del player, aggiorna le vite e lo stato di morte
    private void OnHpChangedHandler(int currentHp, int maxHp)
    {
        maxLives = maxHp;
        currentLives = Mathf.Clamp(currentHp, 0, maxLives);

        UpdateHeartsVisual();
        SetDead(currentLives <= 0);
    }



    // Aggiorna i cuori in base al numero di vite attuali, se muore rende i cuori più trasparenti
    public void UpdateHearts(int lives)
    {
        
        currentLives = Mathf.Clamp(lives, 0, maxLives);

        UpdateHeartsVisual();
        SetDead(currentLives <= 0);
    }


    // Aggiorna le immagini dei cuori in base al num di vite.
    private void UpdateHeartsVisual()
    {
        if (hearts == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            hearts[i].sprite = i < currentLives ? fullHeart : emptyHeart;

            // Se un personaggio ha meno cuori massimi rispetto alla UI, disabilita i cuori in eccesso
            hearts[i].enabled = i < maxLives;
        }
    }

    // Se il player è morto, cuori trasparenti 
    public void SetDead(bool isDead)
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = isDead ? 0.3f : 1f;
    }

    // Handler per l'evento di cambio HP del player
    public PlayerController GetTargetPlayer(){ return targetPlayer; }

}
