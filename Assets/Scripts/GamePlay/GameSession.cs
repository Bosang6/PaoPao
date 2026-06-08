using System.Collections.Generic;
using UnityEngine;


// Questa classe serve per ricordare la mappa scelta quando passo dal menu alla GameScene
// Ricordo anche il player. Bomberman come valore di default
public static class GameSession
{
    public static E_Map SelectedMap = E_Map.Spring;

    public static List<PlayerSlotConfig> PlayerSlots = new List<PlayerSlotConfig>();

    public static void SetupSinglePlayerMatch(E_Map selectedMap, E_Character selectedCharacter)
    {
        SelectedMap = selectedMap;

        PlayerSlots.Clear();

        PlayerSlots.Add(new PlayerSlotConfig(0, selectedCharacter, E_PlayerSlotType.LocalHuman));
        PlayerSlots.Add(new PlayerSlotConfig(1, E_Character.Slime1, E_PlayerSlotType.AI));
        PlayerSlots.Add(new PlayerSlotConfig(2, E_Character.Slime2, E_PlayerSlotType.AI));
        PlayerSlots.Add(new PlayerSlotConfig(3, E_Character.Slime3, E_PlayerSlotType.AI));
    }


}
