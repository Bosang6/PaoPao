using UnityEngine;

namespace Bot
{
    public class BotContext
    {
        //Stato di movimento del bot (exploring: può piazzare una bomba, escaping: sta scappando da una bomba)
        public enum MovingStatus { Exploring, Escaping }

        //Dati di configurazione 
        public CharacterData CharacterData;
        public BotInstanceData InstanceData;
        public GameData GameData;
      
        //Dati di stato e movimento
        public Transform Transform; //Componente unity del bot
        public Vector3 Position;    //Transofrm.Position, più comodo (aggiornato ogni frame in Think)
        public MovingStatus Status;
        public Vector2 PreferredEscapeDirection;
    
        //Dati di gestione delle bombe
        public PlayerBombHandler BombHandler;
        public PhotonBombHandler NetworkBombHandler;
        public Vector3 LastBombPosition;
        public float LastBombTimer;                     //Per quanto tempo il bot deve rimanere in Escaping prima di tornare a Exploring
        public float LastBombFuseTimer;                 //Tempo rimanente prima che la bomba esploda
        public float BombCooldownRemaining;
        public bool BombRequestPending;
    
        //Helper per ottenere la posizione di una cella "centrata e corretta"
        public Vector3 CellPosition => GridUtils.AdjustPosition(Position, GameData.fCellSize);
    }
}
