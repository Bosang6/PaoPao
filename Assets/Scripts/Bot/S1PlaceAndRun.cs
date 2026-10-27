using Bot;
using UnityEngine;

public class S1PlaceAndRun : BotCommonStrategy
{
    protected override Vector2 ChooseMove(BotContext context) => BotToolkit.RandomSafeMove(context);
    protected override bool WantsBomb(BotContext context) => true;
}
