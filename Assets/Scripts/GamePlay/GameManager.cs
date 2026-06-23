using System;
using System.Collections.Generic;
using Unity.Multiplayer.PlayMode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIGameManager uiGameManager;

    [Header("Timer")]
    [SerializeField] private UITimer uiTimer;

    [Header("GameData")]
    [SerializeField] private GameData _gameData;

    private List<PlayerController> players = new List<PlayerController>();
    private bool isMatchEnded = false;

    private void Start()
    {
        foreach (PlayerController player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            RegisterPlayer(player);
        }
        
        AudioManager.Instance.PlayBackgroundMusic(MapManager.Instance.cuurEMap);
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
        
        if (players.Count == 2)
        {
            AudioManager.Instance.PlayFinalBattleMusic();
        }

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

        string localKillCounter = _gameData.localPlayerKill.ToString();

        if (uiGameManager != null)
        {
            if (isWin)
                uiGameManager.ShowWinPanel(finalTime, localKillCounter);
            else
                uiGameManager.ShowLosePanel(finalTime, localKillCounter);
        }
    }
}