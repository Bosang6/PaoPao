using UnityEngine;

namespace Bot
{
    public class S3KamikazeStrategy : BotCommonStrategy
    {
        /*
         * Strategia: vuole avvicinarsi ai nemici e piazzare la bomba
         * - Movimento:
         * -- Priorità 1: evitare le bombe
         * -- Priorità 2: raggiungere il player più vicino
         *
         * - Bombe:
         * -- Priorità 1: colpire il player più vicino
         */
    
        protected override Vector2 ChooseMove(BotContext context)
        {
            if (!BotToolkit.TryNearestCellPlayer(context, out Vector3 target)) return Vector2.zero;
            return BotToolkit.BfsToTile(context, target);
        }

        protected override bool WantsBomb(BotContext context)
        {
            return BotToolkit.IsAdjacentToNearestPlayer(context);
        }

        protected override void ConfigureEscape(BotContext context)
        {
            context.PreferredEscapeDirection = BotToolkit.DirectionAwayFromNearestPlayer(context);
        }
    }
}
