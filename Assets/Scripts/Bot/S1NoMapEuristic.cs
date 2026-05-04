using UnityEngine;

public class S1NoMapEuristic : IBotStrategy
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

    public bool DecideBomb(BotContext context)
    {
        return true;
    }

    public Vector2 DecideMovement(BotContext context)
    {
        Vector2 next = context.LastDirection;

        if (context.Position == context.LastPosition)
        {
            do { next = Directions[Random.Range(0, Directions.Length)]; } 
            while (next == context.LastDirection);
        }

        return next;
    }
}
