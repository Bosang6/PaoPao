using UnityEngine;

public class S0RandomStrategy : IBotStrategy
{
    private static readonly Vector2[] Directions =
    {
        //Minore possibilità di rimanere fermo
        Vector2.up, Vector2.up,
        Vector2.down, Vector2.down,
        Vector2.left, Vector2.left,
        Vector2.right, Vector2.right,
        Vector2.zero,
    };

    //Strategia base, piazza la bomba appena può
    public bool DecideBomb(BotContext context) { return true; }

    public Vector2 DecideMovement(BotContext context)
    {
        Vector2 next = context.LastDirection;

        //Aumenta la probabilità di proseguire dove stava andando
        if (Random.Range(0, 2) == 0 || next == Vector2.zero)
        {
            next = Directions[Random.Range(0, Directions.Length)];
            if(next != Vector2.zero) { context.LastDirection = next; }
        }

        return next;
    }

}
