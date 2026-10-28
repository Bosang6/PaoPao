using UnityEngine;
using UnityEngine.Serialization;

namespace Bot
{
    public enum BotStrategyType
    {
        Random = 0,
        PlaceAndRun = 1,
        Explorer = 2,
        Kamikaze = 3,
        Pacifist = 4
    }

    [CreateAssetMenu(fileName = "BotInstanceData", menuName = "PaoPaoOBJ/InstanceData/BotInstanceData")]
    public class BotInstanceData : PlayerInstanceData
    {
        [Header("Bot settings")]
        public float moveChangeCooldown = 0.2f;
        public float bombCooldown = 1f;
        [HideInInspector, FormerlySerializedAs("difficulty")]
        public int legacyDifficulty;
    }
}