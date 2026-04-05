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


    private void Start()
    {
        UpdateHearts(currentLives);
    }



    private void UpdateHearts(int lives)
    {
        currentLives = Mathf.Clamp(lives, 0, maxLives);

        for (int i = 0; i < hearts.Length; i++)
        {
            if(hearts[i] == null) continue;

            hearts[i].sprite = i < currentLives ? fullHeart : emptyHeart;
        }
    }


}
