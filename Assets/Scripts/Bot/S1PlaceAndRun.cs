using UnityEngine;

public class S1PlaceAndRun : IBotStrategy
{
    private static readonly Vector2[] Directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

    public bool DecideBomb(BotContext context)
    {
        if(context.status == BotContext.STATUS.EXPLORING) {
            context.LastBombPosition = context.Position;
            context.LastBombTimer = context.CharacterData.explosionData.fTotalTimeExplosion() + 0.2f;   //Margine di sicurezza
            context.LastBombDirection = context.LastDirection;  //Si salva la direzione della bomba
            context.LastDirection = -context.LastDirection;     //Scappa nella direzione opposta a quella da cui viene
            context.status = BotContext.STATUS.ESCAPING;
            Debug.Log($"[{context.CharacterData.name}] status: [{context.status}] | BombDir = {context.LastBombDirection}, so LastDir = {context.LastDirection}");
            return true; 
        }
        
        return false;   
    }

    public Vector2 DecideMovement(BotContext context)
    {
        Vector2 next = context.LastDirection;

        //Debug.Log($"[{context.CharacterData.name}] LastBombTimer = {context.LastBombTimer}");

        //Si muove a caso
        if (context.status == BotContext.STATUS.EXPLORING) {
            next = Directions[Random.Range(0, Directions.Length)];
            //Debug.Log($"I'm Exploring, so I choose {next}");
        }

        //Scappa
        if(context.status == BotContext.STATUS.ESCAPING)
        {
            //Se dall'ultimo frame non si è mosso (= è bloccato), sceglie una nuova direzione
            if (Vector3.Distance(context.Position, context.LastPosition) < 0.02f)    //Più robusto per controllo tra Vector3
            {
                //La direzione non dovrà essere ne LastDirection (è bloccato) ne LastBombDirection (c'è la bomba)
                while (next == context.LastDirection || next == context.LastBombDirection) { next = Directions[Random.Range(0, Directions.Length)]; };
                Debug.Log($"I'm locked | Pos({context.Position}) = LastPos({context.LastPosition}) | LastDir = {context.LastDirection}, BombDir = {context.LastBombDirection} -> next = {next}");
            }
            else
            {
                Debug.Log($"I'm Escaping, LastBomb = {context.LastBombDirection}, so I choose {next}");
            }
            Debug.Log($"dir = {next}");
        }

        return next;
    }
}
