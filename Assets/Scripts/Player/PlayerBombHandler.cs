using System.Collections.Generic;
using UnityEngine;

public class PlayerBombHandler : MonoBehaviour
{

    [SerializeField] private PlayerData playerData;

    private int currentBombs = 0;
    private PlayerMove movement;

    void Start() { movement = GetComponent<PlayerMove>(); }

    void Update() { if (Input.GetKeyDown(playerData.bombKey)) { TryPlaceBomb(); } }

    void TryPlaceBomb()
    {
        if (currentBombs >= playerData.maxBombs) return;

        //Posizione (allineata alla griglia) della bomba da piazzare
        Vector3 bombPos = GridUtils.AdjustPosition(transform.position, movement.fCellSize);

        BombController bomb = BombPool.Instance.Get(bombPos);
        if(bomb == null) return;

        bomb.Initialize(PlayerManager.Instance.GetAllPlayers());

        // Sottoscrivi l'evento di ritorno al pool per decrementare il contatore
        bomb.OnBombReturned += ReleaseBomb;

        /*  Registra il listener "ReleaseBomb" come listener dell'evento "OnBombReturned"
         *  (che è un evento, spiegato in BombController > Explode ()). Quando, nel metodo
         *  Explode() di BombController, verrà invocato OnBombReturned?.Invoke(), Unity
         *  invocherà automaticamente la funzione ReleaseBomb() di questo player.
         *  La sintassi += permette di aprire il canale di comunicazione coi listener */

        currentBombs++;
    }

    void ReleaseBomb()
    {
        currentBombs--;
    }

    
}
