using UnityEngine;
namespace Bot
{
    //Raggruppa il loop principale del bot (muovi, piazza bomba, scappa, ...)
    public abstract class BotCommonStrategy : IBotStrategy
    {
        //----------------------------------Metodi comuni, condivisi da ogni strategia----------------------------------
        /*
         * Aggiorna il contesto e decide cosa fare
         * - Se è in Escaping, prosegue la fuga
         * - Se è in Exploring, verifica se può piazzare una bomba
         */
        public void Think(BotContext context)
        {
            //#1 Aggiorna la posizione del bot e il cooldown della bomba
            context.Position = context.Transform.position;
            context.BombCooldownRemaining -= Time.deltaTime;

            
            //#2 Se si è in fase di fuga, aggiorna i timer della bomba e continua la fuga
            if (context.Status == BotContext.MovingStatus.Escaping)
            {
                context.LastBombTimer -= Time.deltaTime;
                context.LastBombFuseTimer -= Time.deltaTime;
                
                //Se i timer arriva a zero (bomba esplosa), torna nello stato di exploring
                if (context.LastBombTimer <= 0f)
                {
                    context.Status = BotContext.MovingStatus.Exploring;
                    context.PreferredEscapeDirection = Vector2.zero;
                }
                return;
            }


            //#3 condizioni dove think non deve intervenire
            bool isMoving = context.CharacterData.isMoving;                         //Se è in "animazione" di movimento
            bool hasCooldown = context.BombCooldownRemaining > 0f;                  //Se è in cooldown di bomba
            bool notNeedToPlaceBomb = !WantsBomb(context);                          //Se la strategia ha deciso che deve mettere una bomba
            bool isNotSafeToPlaceBomb = !BotToolkit.CanPlaceBombAndEscape(context); //Se è in condizione di piazzare la bomba e scappare
            
            if (isMoving || hasCooldown || notNeedToPlaceBomb || isNotSafeToPlaceBomb) return;

            
            //#4 condizioni superate, si può *pensare di* piazzare una bomba e scappare
            
            //Salva l'eventuale posizione della bomba
            context.LastBombPosition = context.CellPosition;
            context.LastBombFuseTimer = context.CharacterData.explosionData.fTimeToExplode;
            context.LastBombTimer = context.CharacterData.explosionData.fTotalTimeExplosion() + 0.2f;
            context.Status = BotContext.MovingStatus.Escaping;
            
            //Sceglie l'eventuale direzione di fuga
            ConfigureEscape(context);

            //Cerca una cella dove scappare
            if (BotToolkit.NearestCellSafe(context) == context.CellPosition)
            {
                //Se non la trova (ottiene la cella di partenza come unica cella safe), annulla lo stato di fuga e rimane in exploring (rinunica alla bomba)
                context.Status = BotContext.MovingStatus.Exploring;
                context.LastBombTimer = 0f;
                context.LastBombFuseTimer = 0f;
                context.PreferredEscapeDirection = Vector2.zero;
                return;
            }
            
            //Se la trova, si mette in attesa di piazzare la bomba
            context.BombRequestPending = true;
        }

        /*
         * Sceglie in che direzione muoversi
         * - Se è in Escaping, cerca tramite BFS la cella sicura più vicina e ci va
         * - Se è in Exploring, seleziona cosa fare tramite ChooseMove, che dipende dalla strategia del bot
         */
        public Vector2 DecideMove(BotContext context)
        {
            Vector2 proposed;
            if (context.Status == BotContext.MovingStatus.Escaping)
            {
                proposed = BotToolkit.BfsToTile(context, BotToolkit.NearestCellSafe(context, context.PreferredEscapeDirection), context.PreferredEscapeDirection);
            }
            else
            {
                proposed = ChooseMove(context);
            }

            //Se la fuga / strategia propone di rimanere fermi e la cella attuale è safe, si rimane fermi
            if (proposed == Vector2.zero && BotToolkit.IsCellSafe(context, context.CellPosition)) { return Vector2.zero; }
            
            //Se la fuga / strategia propone un movimento, si verifica se la posizione proposta è safe ed eventualmente ci si muove
            if (IsMoveSafe(context, proposed))  { return proposed; }
            
            //Se la posizione proposta non è safe, si cerca una posizione sicura e ci si muove
            return BotToolkit.BfsToTile(context, BotToolkit.NearestCellSafe(context, context.PreferredEscapeDirection), context.PreferredEscapeDirection);
        }

        /*
         * Consuma la richiesta di piazzamento della bomba
         */
        public bool DecideBomb(BotContext context)
        {
            //Se non c'è una richiesta di piazzamento della bomba, restituisce false
            if (!context.BombRequestPending) return false;
            
            //Se c'è una richiesta di piazzamento della bomba, lo consuma e restituisce true
            context.BombRequestPending = false;
            context.BombCooldownRemaining = context.InstanceData.bombCooldown;
            return true;
        }
        
        /*
         * Controlla se il movimento scelto è safe (helper per DecideMove)
         */
        private static bool IsMoveSafe(BotContext context, Vector2 move)
        {
            if (move == Vector2.zero) return false;
            float size = context.GameData.fCellSize;
            float stepTime = size / Mathf.Max(0.1f, context.CharacterData.moveSpeed);
            Vector3 destination = context.CellPosition + (Vector3)(move * size);
            return BotToolkit.CanReachCell(context, destination, stepTime * 0.5f);
        }
        //----------------------------------Metodi comuni, condivisi da ogni strategia----------------------------------

        //-------------------------------Metodi custom, personalizzati per ogni strategia-------------------------------
        //Decide in che direzione muoversi quando non si è in fuga
        protected abstract Vector2 ChooseMove(BotContext context);
        //Decide se, data la situazione attuale, il bot vuole provare a piazzare una bomba
        protected abstract bool WantsBomb(BotContext context);
        //Facoltativo, se si vuole customizzare la via di fuga
        protected virtual void ConfigureEscape(BotContext context) { }
        //-------------------------------Metodi custom, personalizzati per ogni strategia-------------------------------

        
    }
}
