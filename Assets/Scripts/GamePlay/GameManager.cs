using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        //Registra tutti i player in scena al momento dell'avvio
        foreach (PlayerController p in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            RegisterPlayer(p);
        }
    }

    private List<PlayerController> _players = new List<PlayerController>();

    public void RegisterPlayer(PlayerController player)
    {
        _players.Add(player);
        player.OnPlayerDied += OnPlayerDied;
    }

    private void OnPlayerDied(PlayerController player)
    {
        _players.Remove(player);

        //Controllo se(rimane un player){ finePartita() }
    }

  
}
