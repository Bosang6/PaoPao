using UnityEngine;

namespace Bot
{
    public class S2ExplorerStrategy : BotCommonStrategy
    {
        /*
         * Strategia: non vuole attaccare i nemici, vule "aprire" la mappa
         * - Movimento:
         * -- Priorità 1: evitare le bombe
         * -- Priorità 2: avvicinarsi a tile breakable
         * 
         * - Bombe:
         * -- Priorità 1: distruggere i breakable
         *
         * Nota: se non trova celle raggiungibili accanto a breakable, si comporta come la strategia kamikaze
         */
        
        protected override Vector2 ChooseMove(BotContext context)
        {
            //Cerca ed eventualmente raggiunge le tile breakable
            if (BotToolkit.TryNearestCellBreakable(context, out Vector3 breakableTarget))
            {
                return BotToolkit.BfsToTile(context, breakableTarget);
            }

            //Se non ci sono tile breakable si comporta da Kamikaze
            return BotToolkit.TryNearestCellPlayer(context, out Vector3 playerTarget) ? BotToolkit.BfsToTile(context, playerTarget) : Vector2.zero;
        }

        protected override bool WantsBomb(BotContext context)
        {
            //Se è vicino a un tile breakable, piazza la bomba
            if (BotToolkit.TryNearestCellBreakable(context, out _))
            {
                return BotToolkit.HasBreakableAdjacent(context, context.CellPosition);
            }

            //Se non ci sono tile breakable si comporta da Kamikaze
            return BotToolkit.IsAdjacentToNearestPlayer(context);
        }

        protected override void ConfigureEscape(BotContext context)
        {
            if (!BotToolkit.TryNearestCellBreakable(context, out _))
            {
                context.PreferredEscapeDirection = BotToolkit.DirectionAwayFromNearestPlayer(context);
            }
        }
    }
}
