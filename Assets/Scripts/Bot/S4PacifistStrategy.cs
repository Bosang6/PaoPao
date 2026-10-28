using UnityEngine;

namespace Bot
{
    public class S4PacifistStrategy : BotCommonStrategy
    {
        /*
         * Strategia: non vuole attaccare i nemici, fino a che non rimane 1vs1
         * - Movimento:
         * -- Priorità 1: evitare le bombe
         *
         * - Bombe:
         * -- Priorità 1: nessuna
         *
         * Nota: se rimane 1vs1 passa a modalità Kamikaze
         */
        protected override Vector2 ChooseMove(BotContext context)
        {
            //Se nPlayer == 2 -> diventa un Kamikaze
            if (BotToolkit.CountPlayers() == 2)
            {
                if (!BotToolkit.TryNearestCellPlayer(context, out Vector3 target)) return Vector2.zero;
                return BotToolkit.BfsToTile(context, target);
            }

            //Se nPlayer > 2 -> "pacifista"
            Vector3 currentCell = context.CellPosition;
            Vector3 safeCell = BotToolkit.NearestCellSafe(context);

            //Se la cella attuale non è pericolosa, rimane fermo
            if (BotToolkit.SameCell(currentCell, safeCell, context.GameData.fCellSize))
            {
                return Vector2.zero;
            }

            //Se la cella attuale è pericolosa, si muove verso quella safe più vicina
            return BotToolkit.BfsToTile(context, safeCell);
        }

        protected override bool WantsBomb(BotContext context)
        {
            return BotToolkit.CountPlayers() == 2 && BotToolkit.IsAdjacentToNearestPlayer(context);
        }

        protected override void ConfigureEscape(BotContext context)
        {
            if (BotToolkit.CountPlayers() == 2)
            {
                context.PreferredEscapeDirection = BotToolkit.DirectionAwayFromNearestPlayer(context);
            }
        }
    }
}
