using Bot;
using UnityEngine;

public interface IBotStrategy
{
    void Think(BotContext context);
    Vector2 DecideMove(BotContext context);
    bool DecideBomb(BotContext context);
}
