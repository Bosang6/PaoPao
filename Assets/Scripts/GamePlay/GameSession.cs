using System.Collections.Generic;
using UnityEngine;


// Questa classe serve per ricordare la mappa scelta quando passo dal menu alla GameScene
// Ricorda anche la configurazione dei player slot, che è necessaria per spawnare i player nella scena di gioco
public static class GameSession
{
    public static E_Map SelectedMap = E_Map.Spring;

    public static List<PlayerSlotConfig> PlayerSlots { get; private set; } = new List<PlayerSlotConfig>();

    // Metodo per impostare la configurazione della partita, inclusa la mappa selezionata e i player slot
    public static void SetMatchConfig(E_Map selectedMap, List<PlayerSlotConfig> playerSlots)
    {
        SelectedMap = selectedMap;

        PlayerSlots.Clear();

        if (playerSlots == null)
        {
            Debug.LogWarning("Player slots non validi.");
            return;
        }

        PlayerSlots.AddRange(playerSlots);
    }


}
