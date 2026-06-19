using UnityEngine;

public class BotContext
{
    public enum STATUS { EXPLORING, ESCAPING };
    
    public CharacterData CharacterData;
    public BotInstanceData InstanceData;

    //Dati riguardo il frame attuale
    public STATUS status;
    public Vector3 Position;
    
    //Dati riguardo l'ultima bomba piazzata
    public Vector3 LastBombPosition;
    public Vector2 LastBombDirection;
    public float LastBombTimer = 0f;

    //Dati riguardo il frame precedente
    public Vector3 LastPosition;
    public Vector2 LastDirection;

}
