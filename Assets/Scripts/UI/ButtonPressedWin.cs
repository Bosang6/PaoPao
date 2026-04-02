using System;
using UnityEngine;

public class ButtonPressedWin : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TitleAnimation titleAnimation;
    [SerializeField] private TitleAnimation titleLoseAnimation;

    public void OnWinButtonPressed()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

 

        if (titleAnimation != null)
            titleAnimation.PlayAnimation();
    }

    public void OnLoseButton()
    {
        losePanel.SetActive(true);
        titleLoseAnimation.PlayAnimation();

    }
}
