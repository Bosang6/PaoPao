using UnityEngine;

namespace Bot
{
    public interface IBotStrategy
    {
        void Think(BotContext context);
        void UpdateTimers(BotContext context, float deltaTime);
        Vector2 DecideMove(BotContext context);
        bool DecideBomb(BotContext context);
    }
}