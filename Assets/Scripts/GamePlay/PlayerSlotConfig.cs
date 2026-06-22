
/*
 * Descrive cosa deve esserci in uno slot della partita
 * Slot 0 --> Bomberman --> Human
 */

[System.Serializable]
public class PlayerSlotConfig
{
    public int slotIndex;           // num slot
    public CharacterData.E_Character cType;   // character
    public PlayerInstanceData.E_PlayerSlotType pType;       // Human / AI

    public PlayerSlotConfig(int slotIndex, CharacterData.E_Character cType, PlayerInstanceData.E_PlayerSlotType pType)
    {
        this.slotIndex = slotIndex;
        this.cType = cType;
        this.pType = pType;
    }
}