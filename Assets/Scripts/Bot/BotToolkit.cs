using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Bot
{
    public static class BotToolkit
    {
        private static readonly Vector2[] Directions = { Vector2.up, Vector2.right, Vector2.down, Vector2.left };

        /*
         * Struttura per la ricerca di percorso
         * - CurrentCell: la cella attuale dalla quale si cerca il percorso
         * - FirstStep: la direzione da prendere per raggiungere la cella
         * - Distance: il numero di tile che separando CurrentCell dalla cella iniziale
         */
        private struct SearchNode
        {
            public readonly Vector2Int CurrentCell;
            public readonly Vector2 FirstStep;
            public readonly int Distance;

            public SearchNode(Vector2Int cell, Vector2 firstStep, int distance)
            {
                CurrentCell = cell;
                FirstStep = firstStep;
                Distance = distance;
            }
        }

        /*
         * Controlla se una cella è sicura
         * - arrivalTime = 0 -> mi interessa sapere se la cella è safe ora
         * - arrivalTime > 0 -> mi interessa sapere se la cella è safe tra arrivalTime secondi
         * Invoca IsCellSafeForDuration, calcolando il tempo impiegato ad attraversare una cella
         */
        public static bool IsCellSafe(BotContext context, Vector3 cell, float arrivalTime = 0f)
        {
            //Tempo necessario per attraversare una tile
            float stepTime = context.GameData.fCellSize / Mathf.Max(0.1f, context.CharacterData.moveSpeed);
            return IsCellSafeForDuration(context, cell, arrivalTime, stepTime);
        }

        /*
         * Controlla se una cella è sicura in un dato lasso temporale
         * - arrivalTime = 0 -> mi interessa sapere se la cella è safe ora
         * - arrivalTime > 0 -> mi interessa sapere se la cella è safe tra arrivalTime secondi
         * - occupancyTime -> tempo impiegato per attraversare una tile
         */
        private static bool IsCellSafeForDuration(BotContext context, Vector3 cell, float arrivalTime, float occupancyTime)
        {
            float cellSize = context.GameData.fCellSize;
            const float safetyMargin = 0.2f;

            //Controlla se nella tile sono previste fiamme
            if (FlamePool.Instance != null)
            {
                foreach (FlameController flame in FlamePool.Instance.ActiveFlames)
                {
                    if (flame != null && flame.RemainingTime + safetyMargin > arrivalTime && SameCell(cell, flame.Position, cellSize))
                    {
                        return false;
                    }
                }
            }

            //Controllo per il multiplayer
            if (PhotonNetwork.InRoom)
            {
                foreach (PhotonFlameVisual flame in Object.FindObjectsByType<PhotonFlameVisual>(FindObjectsSortMode.None))
                {
                    if (flame != null && flame.RemainingTime + safetyMargin > arrivalTime && SameCell(cell, flame.transform.position, cellSize))
                    {
                        return false;
                    }
                }
            }

            //Controlla se nella tile è prevista una bomba (invoca IsBombDangerousAt)
            if (BombPool.Instance != null)
            {
                foreach (BombController bomb in BombPool.Instance.ActiveBombs)
                {
                    if (bomb != null && bomb.Data != null && IsBombDangerousAt(bomb.Data, bomb.Position, bomb.TimeToExplode, cell, arrivalTime, occupancyTime, context))
                    {
                        return false;
                    }
                }
            }

            //Controllo per il multiplayer
            if (PhotonNetwork.InRoom)
            {
                foreach (PhotonBombController bomb in Object.FindObjectsByType<PhotonBombController>(FindObjectsSortMode.None))
                {
                    if (bomb != null && bomb.Data != null && IsBombDangerousAt(bomb.Data, bomb.Position, bomb.TimeToExplode, cell, arrivalTime, occupancyTime, context))
                    {
                        return false;
                    }
                }
            }

            //Tiene in considerazione anche la bomba che si ha intenzione di piazzare ma che non è ancora presente nel pool
            bool justPlacedBomb = (context.Status == BotContext.MovingStatus.Escaping && context.LastBombTimer > 0f);   //Si è appena deciso di piazzarla (non è detto che sia effettivamente piazzata)
            bool alreadyPlaced = HasBombAt(context.LastBombPosition, cellSize);                                         //Per evitare doppi controlli se è effettivamente stata piazzata
            bool isBombDangerous = IsBombDangerousAt(context.CharacterData.explosionData, context.LastBombPosition, context.LastBombFuseTimer, cell, arrivalTime, occupancyTime, context);
            
            if (justPlacedBomb && !alreadyPlaced &&  isBombDangerous) { return false; }

            
            //Se tutti i controlli sono scongiurati, la tile viene considerata safe
            return true;
        }

        /*
         * Controlla se il bot può *fisicamente* entrare nella cella (controllo di collisioni)
         * - Controlla se la cella non è già occupata da un altro player
         * - Controlla se la cella non è già occupata da una bomba
         * - Controlla se la cella è safe (invocando IsCellSafe)
         * Nota: ignoreOtherPlayers serve per ignorare, nei nodi futuri della BFS, la presenza di altri player
         *  poiché ora ostacolano il percorso ma non è detto che lo faranno anche al raggiungimento di quella tile
         */
        public static bool CanReachCell(BotContext context, Vector3 cell, float arrivalTime = 0f, bool ignoreOtherPlayers = false)
        {
            float cellSize = context.GameData.fCellSize;
            
            //Controlla se la cella è già occupata da un player
            if (!ignoreOtherPlayers && PlayerManager.Instance != null && PlayerManager.Instance.IsCellReservedByOther(context.Transform, cell, cellSize)) return false;
            
            //Controlla se la cella è occupata da una bomba
            if (HasBombAt(cell, cellSize)) return false;

            //Controlla se nella cella ci sono collider attivi (es altri Bot)
            Vector2 box = Vector2.one * (cellSize * 0.9f);
            Collider2D[] colliders = Physics2D.OverlapBoxAll(cell, box, 0f, context.GameData.lmCollisionLayer);
            foreach (Collider2D collider in colliders)
            {
                if (collider == null) continue;
                Transform hit = collider.transform;
                if (hit == context.Transform || hit.IsChildOf(context.Transform)) continue;
                if (ignoreOtherPlayers && hit.GetComponentInParent<PlayerMove>() != null) continue;
                return false;
            }

            //Passato il controllo "fisico" (collisioni), restituisce il risultato del controllo "logico" 
            return IsCellSafe(context, cell, arrivalTime);
        }

        /*
         * Restituisce la cella più vicina considerata safe (viene fornito un predicato per il controllo di "safe")
         */
        public static Vector3 NearestCellSafe(BotContext context, Vector2 preferredDirection = default)
        {
            Vector3 current = context.CellPosition;

            //Predicato per il controllo sulla "sicurezza della cella" -> float.MaxValue per verificare se la cella è sicura "per sempre"
            bool Check(Vector3 cell, float arrivalTime) => IsCellSafeForDuration(context, cell, arrivalTime, float.MaxValue);

            //Se TryFindNearestCell ha successo, restituisce il result trovato, altrimenti restituisce la cella attualmente occupata
            return TryFindNearestCell(context, Check, out Vector3 result, out _, preferredDirection) ? result : current;
        }

        /*
         * Restituisce la cella più vicina che abbia nel proprio intorno una tile breakable
         * - Se non esistono tile breakable, restituisce context.CellPosition
         * - Invoca TryNearestCellBreakable, che stabilisce se esiste una tile breakable o no
         */
        public static Vector3 NearestCellBreakable(BotContext context)
        {
            return TryNearestCellBreakable(context, out Vector3 result) ? result : context.CellPosition;
        }

        /*
         * Controlla se trova una cella che abbia nel proprio intorno una tile breakable
         * - Passa un predicato per controllare se la cella ha nel proprio intorno una tile breakable
         * - Diversa da NearestCellBreakable perchè permette di controllare se l'operazione ha avuto successo o no
         */
        public static bool TryNearestCellBreakable(BotContext context, out Vector3 result)
        {
            //Predicato per controllare se la cella ha nel proprio intorno una cella breakable
            bool Check(Vector3 cell) => HasBreakableAdjacent(context, cell);

            return TryFindNearestCell(context, Check, out result);
        }

        /*
         * Restituisce la cella più vicina che abbia nel proprio intorno un player
         * - Se non esistono player, restituisce context.CellPosition
         * - Invoca TryNearestCellPlayer, che stabilisce se esiste una tile con player o no
         */
        public static Vector3 NearestCellPlayer(BotContext context)
        {
            return TryNearestCellPlayer(context, out Vector3 result) ? result : context.CellPosition;
        }

        /*
         * Controlla se trova una cella che abbia nel proprio intorno un player
         * - Passa un predicato per controllare se la cella ha nel proprio intorno un player
         * - Diversa da NearestCellPlayer perchè permette di controllare se l'operazione ha avuto successo o no
         */
        public static bool TryNearestCellPlayer(BotContext context, out Vector3 result)
        {
            //Trova il player più vicino
            Transform player = FindNearestPlayer(context);
            
            //Se non trova un player, restituisce la posizione attuale
            if (player == null)
            {
                result = context.CellPosition;
                return false;
            }
            Vector3 playerCell = GridUtils.AdjustPosition(player.position, context.GameData.fCellSize);
            
            //Se trova un player, restituisce la cella più vicina al player (passa un predicato per determinare il concetto di "vicino")
            bool Check(Vector3 cell) => IsAdjacent(cell, playerCell, context.GameData.fCellSize);
            return TryFindNearestCell(context, Check , out result);
        }

        /*
         * Controlla se è possibile "creare" un percorso verso un player distruggendo dei breakable
         * - BreakableCell: la prima cella con una breakable (andrà distrutta)
         * - StandingCell: la cella precedente alla BreakableCell (il bot deve raggiungerla per distruggere BreakableCell)
         */
        public static bool TryFindBreakableBlockingPlayerRoute(BotContext context, out Vector3 breakableCell, out Vector3 standingCell)
        {
            //#0 Preparazione
            
            //In caso di fallimento, breakableCell e standingCell mantengono valori safe
            breakableCell = context.CellPosition;
            standingCell = context.CellPosition;

            //Cerca il player più vicino, se non ce ne sono termina il metodo
            Transform player = FindNearestPlayer(context);
            if (player == null) return false;

            //Memorizza dimensione della griglia e vincoli sulla ricerca
            float size = context.GameData.fCellSize;
            Vector3 playerCell = GridUtils.AdjustPosition(player.position, size);
            Vector2Int start = ToCell(context.CellPosition, size);
            GetBounds(out Vector2Int min, out Vector2Int max);

            
            //#1 BFS (simile a TryFindNearestCell) 
            Queue<Vector2Int> open = new Queue<Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int> { start };
            Dictionary<Vector2Int, Vector2Int> parents = new Dictionary<Vector2Int, Vector2Int>();
            Dictionary<Vector2Int, int> distances = new Dictionary<Vector2Int, int> { [start] = 0 };
            open.Enqueue(start);

            float stepTime = size / Mathf.Max(0.1f, context.CharacterData.moveSpeed);
            Vector2Int goal = start;
            bool foundGoal = false;

            while (open.Count > 0)
            {
                Vector2Int current = open.Dequeue();
                int currentDistance = distances[current];

                foreach (Vector2 direction in Directions)
                {
                    Vector2Int next = current + Vector2Int.RoundToInt(direction);
                    if (next.x < min.x || next.x > max.x || next.y < min.y || next.y > max.y || visited.Contains(next)) continue;

                    int distance = currentDistance + 1;
                    float arrivalTime = Mathf.Max(0f, (distance - 0.5f) * stepTime);
                    Vector3 world = ToWorld(next, context.Position.z, size);
                    if (!CanTraverseBreachRoute(context, world, playerCell, arrivalTime)) continue;

                    visited.Add(next);
                    parents[next] = current;
                    distances[next] = distance;
                    if (IsAdjacent(world, playerCell, size))
                    {
                        goal = next;
                        foundGoal = true;
                        break;
                    }

                    open.Enqueue(next);
                }

                if (foundGoal) break;
            }

            //Non esistono percorsi per raggiungere il player
            if (!foundGoal) return false;

            
            //#2 Ricostruzione path
            List<Vector2Int> path = new List<Vector2Int> { goal };
            while (path[path.Count - 1] != start) path.Add(parents[path[path.Count - 1]]);
            path.Reverse();

            //Cerca il primo breakable lungo il percorso (sarà breakableCell) e la tile precedente (standingCell)
            for (int i = 1; i < path.Count; i++)
            {
                Vector3 cell = ToWorld(path[i], context.Position.z, size);
                if (!IsBreakableCell(context, cell)) continue;

                breakableCell = cell;
                standingCell = ToWorld(path[i - 1], context.Position.z, size);
                return true;
            }

            return false;
        }

        /*
         * Helper per TryFindBreakableBlockingPlayerRoute, verifica se una cella è utilizzabile per il percorso ipotetico
         * - Se è la cella che contiene il player -> non serve per il percorso (bisogna fermarsi la tile prima) [#1]
         * - Se la cella contiene una bomba -> non è safe [#1]
         * - Se la cella contiene un wall -> non è percorribile [#2]
         * - Se è breakable -> controlla se è safe (no bombe / fiamme) e in caso la approva [#3] (utilizza IsBreakableCell())
         * - Se non è breakable -> fa un controllo fisico (per vedere se è vuota) ed eventualmente la approva
         */
        private static bool CanTraverseBreachRoute(BotContext context, Vector3 cell, Vector3 playerCell, float arrivalTime)
        {
            float size = context.GameData.fCellSize;
            
            //[#1]
            if (SameCell(cell, playerCell, size) || HasBombAt(cell, size)) return false;    

            //[#2]
            Vector2 box = Vector2.one * (size * 0.9f);
            if (BreakableTilemap.HasWallAtWorldPosition(cell) || Physics2D.OverlapBox(cell, box, 0f, context.CharacterData.explosionData.lmWallLayer) != null)
            {
                return false;
            }

            //[#3]
            bool isBreakable = IsBreakableCell(context, cell);
            if (!IsCellSafe(context, cell, arrivalTime)) return false;
            if (isBreakable) return true;

            //[#4]
            Collider2D[] colliders = Physics2D.OverlapBoxAll(cell, box, 0f, context.GameData.lmCollisionLayer);
            foreach (Collider2D collider in colliders)
            {
                if (collider == null) continue;
                Transform hit = collider.transform;
                if (hit == context.Transform || hit.IsChildOf(context.Transform)) continue;
                return false;
            }

            return true;
        }

        /*
         * Helper per CanTraverseBreachRoute e TryFindBreakableBlockingPlayerRoute, verifica se la tile è breakable
         * Nota: verifica se è *attualmente* breakable, quindi se il collider è ancora attivo
         */
        private static bool IsBreakableCell(BotContext context, Vector3 cell)
        {
            if (!BreakableTilemap.HasBreakableAtWorldPosition(cell)) return false;
            Vector2 box = Vector2.one * (context.GameData.fCellSize * 0.9f);
            return Physics2D.OverlapBox(cell, box, 0f, context.CharacterData.explosionData.lmBreakableLayer) != null;
        }

        /*
         * Restituisce la direzione da seguire per raggiungere targetTile
         * - Se targetTile non è raggiungibile, restituisce Vector2.zero
         * - Utilizza TryFindNearestCell per calcolare il percorso, ma poi restituisce solo il prossimo step
         * - Utilizza un predicato per stabilire se le celle coincidono
         */
        public static Vector2 BfsToTile(BotContext context, Vector3 targetTile, Vector2 preferredDirection = default)
        {
            bool Check(Vector3 cell, float _) => SameCell(cell, targetTile, context.GameData.fCellSize);

            return TryFindNearestCell(context, Check, out _, out Vector2 firstStep, preferredDirection) ? firstStep : Vector2.zero;
        }

        /*
         * Sceglie una direzione casuale tra quelle percorribili
         */
        public static Vector2 RandomSafeMove(BotContext context)
        {
            List<Vector2> options = new List<Vector2>();
            float size = context.GameData.fCellSize;
            float stepTime = size / Mathf.Max(0.1f, context.CharacterData.moveSpeed);
            
            //Controlla tutte le direzioni e salva quelle raggiungibili
            foreach (Vector2 direction in Directions)
            {
                Vector3 target = context.CellPosition + (Vector3)(direction * size);
                if (CanReachCell(context, target, stepTime * 0.5f)) options.Add(direction);
            }
            
            //Restituisce una direzione casuale tra quelle raggiungibili (o Vector2.zero se non ne trova)
            return options.Count == 0 ? Vector2.zero : options[Random.Range(0, options.Count)];
        }

        /*
         * Ipotizza il piazzamento di una bomba e verifica se esistono tile sicure da raggiungere 
         */
        public static bool CanPlaceBombAndEscape(BotContext context)
        {
            //Controlla se è possibile piazzare una bomba
            if(!(context.NetworkBombHandler != null || (context.BombHandler != null && context.BombHandler.CanPlaceBomb))){ return false; }

            //Deve effettuare una simulazione, quindi salva i dati attuali per ripristinarli al termine della simulazione
            Vector3 origin = context.CellPosition;
            BotContext.MovingStatus previousStatus = context.Status;
            Vector3 previousBombPosition = context.LastBombPosition;
            float previousFuseTimer = context.LastBombFuseTimer;
            float previousBombTimer = context.LastBombTimer;
            
            //Simula il piazzamento di una bomba
            context.Status = BotContext.MovingStatus.Escaping;
            context.LastBombPosition = origin;
            context.LastBombFuseTimer = context.CharacterData.explosionData.fTimeToExplode;
            context.LastBombTimer = context.CharacterData.explosionData.fTotalTimeExplosion() + 0.2f;
            
            //Cerca una casella sicura supponendo di aver piazzato una bomba nella cella attuale
            //Nota: la si cerca diversa da quella attuale, altrimenti vorrebbe dire che NearestCellSafe non ha trovato una tile e ha restituito context.CellPosition
            bool foundEscape = !SameCell(NearestCellSafe(context), origin, context.GameData.fCellSize);
            
            //Ripristina la condizione iniziale
            context.Status = previousStatus;
            context.LastBombPosition = previousBombPosition;
            context.LastBombFuseTimer = previousFuseTimer;
            context.LastBombTimer = previousBombTimer;
            
            //Restituisce il risultato della simulazione
            return foundEscape;
        }

        /*
         * Verifica se nell'intorno è presente una cella brekable
         * Viene usata come funzione di predicato in TryNearestCellBreakable
         */
        public static bool HasBreakableAdjacent(BotContext context, Vector3 position)
        {
            float size = context.GameData.fCellSize;
            Vector2 box = Vector2.one * (size * 0.9f);
            foreach (Vector2 direction in Directions)
            {
                //Memorizza l'ipotetica direzione
                Vector3 target = position + (Vector3)(direction * size);
                
                //Se in quella direzione c'è un muro
                bool hasWall = BreakableTilemap.HasWallAtWorldPosition(target) || Physics2D.OverlapBox(target, box, 0f, context.CharacterData.explosionData.lmWallLayer) != null;
                
                //Se in quella direzione c'è un breakable
                bool hasBreakable = BreakableTilemap.HasBreakableAtWorldPosition(target);
                
                //Se in quella direzione c'è un muro o non c'è un breakable, si passa alla direzione successiva
                if (hasWall || !hasBreakable) continue;

                //Effettua anche un controllo fisico sul collider breakable
                if (Physics2D.OverlapBox(target, box, 0f, context.CharacterData.explosionData.lmBreakableLayer) != null)
                {
                    return true;
                }
            }
            
            //Se si ha fallito in tutte le direzioni restituisce false
            return false;
        }

        /*
         * Verifica se nell'intorno è presente un player
         * Usata nelle strategie kamikaze e pacifist 
         */
        public static bool IsAdjacentToNearestPlayer(BotContext context)
        {
            Transform player = FindNearestPlayer(context);
            return player != null && IsAdjacent(context.CellPosition, GridUtils.AdjustPosition(player.position, context.GameData.fCellSize), context.GameData.fCellSize);
        }

        /*
         * Trova il player più vicino e sceglie una direzione per allontanarsi da lui
         * - Se non trova un altro player, restituisce Vector2.zero
         * - Restituisce la direzione che da maggiore distanza
         * -- Se la differenza orizzontale è maggiore, restituisce destra o sinistra
         * -- Se la differenza verticale è maggiore, restituisce sopra o sotto
         */
        public static Vector2 DirectionAwayFromNearestPlayer(BotContext context)
        {
            //Trova la posizione del player più vicino
            Transform player = FindNearestPlayer(context);
            if (player == null) return Vector2.zero;
            Vector3 delta = context.CellPosition - GridUtils.AdjustPosition(player.position, context.GameData.fCellSize);
            
            //Se la differenza orizzontale è maggiore, restituisce destra o sinistra
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y)) return new Vector2(Mathf.Sign(delta.x), 0f);
            
            //Se la differenza verticale è maggiore, restituisce sopra o sotto
            if (Mathf.Abs(delta.y) > 0f) return new Vector2(0f, Mathf.Sign(delta.y));
            
            return Vector2.zero;
        }

        /*
         * Conta i giocatori attualmente presenti nella partita
         * - Se si è offline, usa PlayerManager per ottenere i giocatori della partita locale e conta quelli non nulli
         * - Se si è online, PhotonNetwork.InRoom cerca tutti i componenti PhotonPlayerController presenti nella scena e conta quelli non nulli
         */
        public static int CountPlayers()
        {
            //Multiplayer
            if (PhotonNetwork.InRoom)
            {
                int count = 0;
                foreach (PhotonPlayerController player in Object.FindObjectsByType<PhotonPlayerController>(FindObjectsSortMode.None))
                {
                    if (player != null) count++;
                }
                return count;
            }
            
            //Singleplayer
            if (PlayerManager.Instance == null) return 0;
            int localCount = 0;
            foreach (Transform player in PlayerManager.Instance.GetAllPlayers())
            {
                if (player != null) localCount++;
            }
            return localCount;
        }

        /*
         * Adattatore per TryFindNearestCell, invocati quando interessa avere informazioni riguardanti solo la cella
         * - BotContext context: contesto dal quale il bot prende le informazioni
         * - System.Predicate<Vector3> isGoal: predicato che prende in input una cella e restituisce true o false
         * -- I predicati vengono usati per verificare se determinate condizioni sono rispettate o no
         * -- Es di predicati focalizzati solo sulla cella: HasBreakableAdjacent, IsAdjacent
         * - Come valore out interessa solo la destinazione, senza avere informazioni sul firstStep
         */
        private static bool TryFindNearestCell(BotContext context, System.Predicate<Vector3> isGoal, out Vector3 destination)
        {
            //Il predicato viene adattato in modo che matchi con System.Func<Vector3, float, bool>
            bool Check(Vector3 cell, float _) => isGoal(cell);
            
            return TryFindNearestCell(context, Check, out destination, out _, Vector2.zero);
        }

        /*
         * Il motore della ricerca: verifica ed eventualmente restituisce la cella più vicina che soddisfa un requisito
         * - BotContext context: contesto dal quale il bot prende le informazioni
         * - System.Func<Vector3, float, bool> isGoal: predicato che prende in input una cella e un tempo e restituisce true o false
         * -- I predicati vengono usati per verificare se determinate condizioni sono rispettate o no
         * -- Il float indica il tempo massimo entro il quale la cella deve rispettare il predicato
         * -- Es di predicati focalizzati su cella e tempo: IsCellSafeForDuration
         * - Come valore out interessa la destinazione e la direzione da seguire per raggiungere tale destinazione
         * - Aggiunto un preferredDirection per ottimizzare la ricerca e favorire la continuità della fuga
         */
        private static bool TryFindNearestCell(BotContext context, System.Func<Vector3, float, bool> isGoal, out Vector3 destination, out Vector2 firstStep, Vector2 preferredDirection = default)
        {
            //#1 preparazione della ricerca
            float size = context.GameData.fCellSize;
            Vector3 startPosition = context.CellPosition;
            Vector2Int start = ToCell(startPosition, size);
            GetBounds(out Vector2Int min, out Vector2Int max);

            Queue<SearchNode> open = new Queue<SearchNode>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int> { start };
            open.Enqueue(new SearchNode(start, Vector2.zero, 0));
            
            
            //#2 controlla se la cella attuale soddisfa già il predicato, in tal caso non serve muoversi
            if (isGoal(startPosition, 0f))
            {
                destination = startPosition;
                firstStep = Vector2.zero;
                return true;
            }

            
            //#3 esplorazione
            float stepTime = size / Mathf.Max(0.1f, context.CharacterData.moveSpeed);   //Stima di quanto ci mette il bot ad attraversare una cella
            
            //Avvia una BFS, visita i nodi inserendoli in una coda man mano che li incontra, in modo da avere sempre il percorso minore
            while (open.Count > 0)
            {
                SearchNode current = open.Dequeue();
                foreach (Vector2 direction in OrderedDirections(preferredDirection))
                {
                    //Controlla se la cella è in-bound e se non è già stata visitata, altrimenti la skippa
                    Vector2Int next = current.CurrentCell + Vector2Int.RoundToInt(direction);
                    if (next.x < min.x || next.x > max.x || next.y < min.y || next.y > max.y || !visited.Add(next)) continue;

                    //Aggiorna il numero di passi che separano la cella attuale e quella di partenza
                    int distance = current.Distance + 1;
                    Vector3 world = ToWorld(next, context.Position.z, size);
                    float arrivalTime = Mathf.Max(0f, (distance - 0.5f) * stepTime);
                    
                    //Controlla se la cella è raggiungibile (fisicamente e logicamente), altrimenti la skippa
                    //IgnoreOtherPlayers dev'essere attivo solo per i nodi con distanza > 1, poichè potrebbero liberarsi nel frattempo
                    if (!CanReachCell(context, world, arrivalTime, distance > 1)) continue;

                    //Se è il primo passo, memorizza la direzione scelta (andrà restituita), altrimenti la mantiene invariata
                    Vector2 routeFirstStep = current.Distance == 0 ? direction : current.FirstStep;
                    
                    //Se la cella raggiunta soddisfa il predicato, viene restituita come destinazione
                    if (isGoal(world, arrivalTime))
                    {
                        destination = world;
                        firstStep = routeFirstStep;
                        return true;
                    }
                    
                    //Altrimenti, inserisce la cella in coda per esplorare i vicini
                    open.Enqueue(new SearchNode(next, routeFirstStep, distance));
                }
            }

            //Se la coda termina senza trovare una cella che soddisfa il predicato, si restituisce la cella iniziale
            destination = startPosition;
            firstStep = Vector2.zero;
            return false;
        }

        /*
         * Controlla se una bomba può essere pericolosa in un certo range di tempo
         * Per prima cosa controlla i tempi
         * - data == null -> mancano i dati dell'esplosione, la funzione restituisce false
         * - se il momento di esplosione supera il tempo di arrivo + il tempo di occupazione + un margine di sicurezza -> la bomba non è pericolosa quindi restituisce false
         * - se l'arrivo alla cella è previsto dopo il tempo di esplosione + la durata delle fiamme -> le fiamme e la bomba non sono pericolose quindi restituisce false
         * Questi sono *controlli preliminari* senza la conoscenza della posizione della bomba e delle celle
         * Se i controlli preliminari falliscono, si controlla effettivamente se la cella è all'interno del range di esplosione
         */
        private static bool IsBombDangerousAt(ExplosionData data, Vector3 origin, float fuseTime, Vector3 cell, float arrivalTime, float occupancyTime, BotContext context)
        {
            //Controlli preliminari
            if (data == null || fuseTime >= arrivalTime + occupancyTime + 0.2f || fuseTime + data.fFlameDuration <= arrivalTime) return false;
            
            //Controllo effettivo tenendo in considerazione le posizioni di bomba e cella
            return IsInBlast(data, origin, cell, context);
        }

        /*
         * Controlla se una cella si trova nell'area di esplosione di una bomba
         */
        private static bool IsInBlast(ExplosionData data, Vector3 origin, Vector3 cell, BotContext context)
        {
            float size = context.GameData.fCellSize;
            origin = GridUtils.AdjustPosition(origin, size);
            if (SameCell(origin, cell, size)) return true;
            Vector2 box = Vector2.one * (size * 0.9f);
            
            //Scorre ogni direzione per bomba.raggioDiEsplosione per vedere se colpisce la cella
            foreach (Vector2 direction in Directions)
            {
                for (int distance = 1; distance <= data.iRange; distance++)
                {
                    Vector3 rayCell = origin + (Vector3)(direction * (size * distance));
                    
                    //Se incontra un muro, non prosegue oltre
                    if (BreakableTilemap.HasWallAtWorldPosition(rayCell) || Physics2D.OverlapBox(rayCell, box, 0f, data.lmWallLayer) != null) break;
                    
                    //Se incontra la cella attenzionata, il metodo termina
                    if (SameCell(rayCell, cell, size)) return true;

                    //Se incontra un breakable, non prosegue oltre
                    if (BreakableTilemap.HasBreakableAtWorldPosition(rayCell) || Physics2D.OverlapBox(rayCell, box, 0f, data.lmBreakableLayer) != null) break;
                }
            }
            return false;
        }

        /*
         * Verifica se nella cella indicata è presente una bomba
         */
        private static bool HasBombAt(Vector3 cell, float size)
        {
            //Controlla il pool locale
            if (BombPool.Instance != null)
            {
                foreach (BombController bomb in BombPool.Instance.ActiveBombs) 
                { 
                    if (bomb != null && SameCell(bomb.Position, cell, size)) { return true; }
                }            
            }
            
            //Controlla il pool online
            if (PhotonNetwork.InRoom)
            {
                foreach (PhotonBombController bomb in Object.FindObjectsByType<PhotonBombController>(FindObjectsSortMode.None))
                {
                    if (bomb != null && SameCell(bomb.Position, cell, size)) { return true; }
                }
            }
            return false;
        }

        /*
         * Cerca e restituisce la posizione del player più vicino al bot
         * - Utilizza ConsiderPlayer come helper per aggiornare il miglior candidato
         * - Se non trova alcun player, restituisce null
         */        
        private static Transform FindNearestPlayer(BotContext context)
        {
            Transform nearest = null;
            float bestDistance = float.MaxValue;
            
            //Cerca nel manager locale
            if (PlayerManager.Instance != null)
            {
                foreach (Transform player in PlayerManager.Instance.GetAllPlayers())
                {
                    ConsiderPlayer(context, player, ref nearest, ref bestDistance);
                }
            }
            
            //Cerca nel manager del multiplayer
            if (PhotonNetwork.InRoom)
            {
                foreach (PhotonPlayerController player in Object.FindObjectsByType<PhotonPlayerController>(FindObjectsSortMode.None))
                {
                    if (player != null) { ConsiderPlayer(context, player.transform, ref nearest, ref bestDistance); }
                }
            }
            
            return nearest;
        }
        
        /*
         * Helper usato da FindNearestPlayer per aggiornare il miglior candidato
         */
        private static void ConsiderPlayer(BotContext context, Transform candidate, ref Transform nearest, ref float bestDistance)
        {
            if (candidate == null || candidate == context.Transform) return;
            float distance = (candidate.position - context.Position).sqrMagnitude;
            if (distance >= bestDistance) return;
            nearest = candidate;
            bestDistance = distance;
        }

        /*
         * Controlla se le due celle sono adiacenti
         * Traduce entrambe le posizioni da world position a celle e successivamente le confronta
         * - Somma la distanza orizzontale e quella verticale
         * - Se la somma è == 1, allora le celle condividono un lato
         */
        private static bool IsAdjacent(Vector3 a, Vector3 b, float size)
        {
            Vector2Int first = ToCell(a, size);
            Vector2Int second = ToCell(b, size);
            return Mathf.Abs(first.x - second.x) + Mathf.Abs(first.y - second.y) == 1;
        }

        /*
         * Stabilisce i confini della griglia entro i quali TryFindNearestCell può controllare
         * - Se trova la mappa attiva, usa i limiti dell’arena (ArenaMin e ArenaMax)
         * - Se non trova la mappa attiva, ci sono i limiti hardcodati alle ultime versioni delle mappe [27/10/2026]
         */
        private static void GetBounds(out Vector2Int min, out Vector2Int max)
        {
            MapComponent map = MapManager.Instance != null ? MapManager.Instance.CurrentMapComponent : null;

            if (map != null)
            {
                min = map.ArenaMin;
                max = map.ArenaMax;
                return;
            }

            min = new Vector2Int(-6, -5);
            max = new Vector2Int(6, 5);
        }

        /*
         * Converte da coordinate mondo (Vector3) a coordinate griglia (Vector2Int)
         */
        private static Vector2Int ToCell(Vector3 world, float size) 
        {
            return new Vector2Int(Mathf.RoundToInt(world.x / size), Mathf.RoundToInt(world.y / size));
        }

        /*
         * Converte da coordinate griglia (Vector2Int) a coordinate mondo (Vector3)
         */
        private static Vector3 ToWorld(Vector2Int cell, float z, float size)
        {
            return new Vector3(cell.x * size, cell.y * size, z);
        }

        /*
         * Verifica se due posizioni appartengono alla stessa cella
         */
        public static bool SameCell(Vector3 a, Vector3 b, float size)
        {
            return (GridUtils.AdjustPosition(a, size) - GridUtils.AdjustPosition(b, size)).sqrMagnitude < 0.01f;
        }

        /*
         * Restituisce un vettore di direzioni, ordinandole in base alla preferita
         * Nota: la preferita è utilizzata per mantenere la coerenza di fuga
         * Se preferred == Vector2.Zero, restituisce Directions nell'ordine di default (quindi in base a come sono dichiarate)
         */
        private static Vector2[] OrderedDirections(Vector2 preferred)
        {
            if (preferred == Vector2.zero) return Directions;
            Vector2[] result = new Vector2[Directions.Length];
            result[0] = preferred;
            int index = 1;
            foreach (Vector2 direction in Directions) { if (direction != preferred) { result[index++] = direction; } }
            return result;
        }
    }
}
