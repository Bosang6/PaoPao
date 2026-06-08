using System.Collections.Generic;
using UnityEngine;

/*
 * ScriptableObject che contiene la configurazione dei player slot per una partita single player.
 * Il player human è sempre nello slot 0, mentre gli slot successivi sono occupati dai nemici AI configurati nell'Inspector.
 */


[CreateAssetMenu(fileName = "SinglePlayerMatchPreset", menuName = "PaoPaoOBJ/SinglePlayerMatchPreset")]
public class SinglePlayerMatchPreset : ScriptableObject
{
    [Header("AI Enemies")]
    [SerializeField] private List<E_Character> aiEnemies = new List<E_Character>();

    public List<PlayerSlotConfig> BuildSlots(E_Character playerCharacter)
    {
        List<PlayerSlotConfig> slots = new List<PlayerSlotConfig>();

        // Slot 0 = player umano scelto dal menu
        slots.Add(new PlayerSlotConfig(0, playerCharacter, E_PlayerSlotType.LocalHuman));

        // Slot successivi = nemici AI configurati nell'Inspector
        for (int i = 0; i < aiEnemies.Count; i++)
        {
            int slotIndex = i + 1;
            slots.Add(new PlayerSlotConfig(slotIndex, aiEnemies[i], E_PlayerSlotType.AI));
        }

        return slots;
    }
}