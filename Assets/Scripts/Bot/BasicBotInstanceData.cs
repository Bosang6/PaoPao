using UnityEngine;

[CreateAssetMenu(fileName = "BasicBotInstanceData", menuName = "PaoPaoOBJ/InstanceData/BasicBotInstanceData")]
public class BasicBotInstanceData : PlayerInstanceData
{
    [Header("Basic Bot")]
    public float moveChangeCooldown;
    public float bombCooldown;
    //dipendono dalla difficoltà, quindi diversi per ogni partita (quindi per ogni istanza)
}
