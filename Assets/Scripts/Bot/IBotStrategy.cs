using UnityEngine;

public interface IBotStrategy
{
    Vector2 DecideMovement(BotContext context);
    bool DecideBomb(BotContext context);
}
