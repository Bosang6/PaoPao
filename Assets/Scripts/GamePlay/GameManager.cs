using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIGameManager uiGameManager;

    [Header("Timer")]
    [SerializeField] private UITimer uiTimer;

    private List<PlayerController> players = new List<PlayerController>();
    private bool isMatchEnded = false;

    private void Start()
    {
        foreach (PlayerController player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            RegisterPlayer(player);
        }
    }


    public void RegisterPlayer(PlayerController player)
    {
        if (player == null || players.Contains(player)) return;

        players.Add(player);
        player.OnPlayerDied += OnPlayerDied;
    }


    private void OnPlayerDied(PlayerController player)
    {
        if (isMatchEnded) return;

        if (player != null)
        {
            player.OnPlayerDied -= OnPlayerDied;
            players.Remove(player);
        }

        bool humanAlive = false;
        int aliveCount = players.Count;

        foreach (PlayerController currentPlayer in players)
        {
            if (currentPlayer.IsHuman)
            {
                humanAlive = true;
                break;
            }
        }

        if (!humanAlive)
        {
            EndMatch(false);
            return;
        }

        if (aliveCount == 1 && humanAlive)
        {
            EndMatch(true);
        }
    }


    private void EndMatch(bool isWin)
    {
        if (isMatchEnded) return;

        isMatchEnded = true;

        if (uiTimer != null)
        {
            uiTimer.StopTimer();
        }

        string finalTime = uiTimer != null ? uiTimer.GetFormattedTime() : "00:00";

        if (uiGameManager != null)
        {
            if (isWin)
                uiGameManager.ShowWinPanel(finalTime);
            else
                uiGameManager.ShowLosePanel(finalTime);
        }
    }
}