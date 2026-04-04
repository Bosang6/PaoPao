using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    private List<Transform> players = new List<Transform>();

    private void Awake()
    {
        //Singleton
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    //Registra un nuovo giocatore
    public void Register(Transform player) { if (!players.Contains(player)) { players.Add(player); } }

    //Rimuove un giocatore
    public void Unregister(Transform player) { players.Remove(player); }

    public List<Transform> GetAllPlayers() => players;
}
