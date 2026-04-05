using UnityEngine;
using UnityEngine.UI;


public class UIPlayerLives : MonoBehaviour
{

    [Header("Heart Images")]
    [SerializeField] private Image[] hearts;

    [Header("Heart Sprite")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    [Header("Test")]
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int currentLives = 3;

    [Header("UI Fade")]
    [SerializeField] private CanvasGroup canvasGroup;



    private void Start()
    {
        UpdateHearts(currentLives);
        SetDead(currentLives <= 0);
    }

    // Funzione che viene chiamata solo nell'editor, utile per debug quando cambia qualcosa nell'ispector
    private void OnValidate()
    {
        UpdateHearts(currentLives);
        SetDead(currentLives <= 0);

    }

    private void UpdateHearts(int lives)
    {
        currentLives = Mathf.Clamp(lives, 0, maxLives);

        for (int i = 0; i < hearts.Length; i++)
        {
            if(hearts[i] == null) continue;

            hearts[i].sprite = i < currentLives ? fullHeart : emptyHeart;
        }

        SetDead(currentLives <= 0);
    }


    public void SetDead(bool isDead)
    {
        if (canvasGroup == null) return;
        
        canvasGroup.alpha = isDead ? 0.3f : 1f;
    }


}
