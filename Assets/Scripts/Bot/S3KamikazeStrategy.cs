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
         * -- Priorità 2: distruggere i breakable che bloccano la BFS verso il player
         */
    
        protected override Vector2 ChooseMove(BotContext context)
        {
            if (BotToolkit.TryNearestCellPlayer(context, out Vector3 playerTarget))
            {
                return BotToolkit.BfsToTile(context, playerTarget);
            }

            //Se non è disponibile un percorso a causa di breakable, lo crea rompendoli
            if (BotToolkit.CountPlayers() <= 1 || !BotToolkit.TryFindBreakableBlockingPlayerRoute(context, out _, out Vector3 standingCell))
            {
                return Vector2.zero;
            }

            //Se non esiste un percorso realizzabile, rimane fermo
            return BotToolkit.BfsToTile(context, standingCell);
        }

        protected override bool WantsBomb(BotContext context)
        {
            if (BotToolkit.TryNearestCellPlayer(context, out _))
            {
                return BotToolkit.IsAdjacentToNearestPlayer(context);
            }

            if (BotToolkit.CountPlayers() <= 1 || !BotToolkit.TryFindBreakableBlockingPlayerRoute(context, out _, out Vector3 standingCell))
            {
                return false;
            }

            return (context.CellPosition - standingCell).sqrMagnitude < 0.01f;
        }

        protected override void ConfigureEscape(BotContext context)
        {
            context.PreferredEscapeDirection = BotToolkit.DirectionAwayFromNearestPlayer(context);
        }
    }
}
