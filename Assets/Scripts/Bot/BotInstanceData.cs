using UnityEngine;

[CreateAssetMenu(fileName = "BotInstanceData", menuName = "PaoPaoOBJ/InstanceData/BotInstanceData")]
public class BotInstanceData : PlayerInstanceData
{
    [Header("Bot")]
    public float moveChangeCooldown;
    public float bombCooldown;
    public int difficulty;
    //dipendono dalla difficoltà, quindi diversi per ogni partita (quindi per ogni istanza)
}
