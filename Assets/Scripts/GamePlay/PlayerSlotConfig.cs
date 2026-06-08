
/*
 * Descrive cosa deve esserci in uno slot della partita
 * Slot 0 --> Bomberman --> Human
 */

[System.Serializable]
public class PlayerSlotConfig
{
    public int slotIndex;           // num slot
    public E_Character character;   // character
    public E_PlayerSlotType slotType;       // Human / AI

    public PlayerSlotConfig(int slotIndex, E_Character character, E_PlayerSlotType slotType)
    {
        this.slotIndex = slotIndex;
        this.character = character;
        this.slotType = slotType;
    }
}