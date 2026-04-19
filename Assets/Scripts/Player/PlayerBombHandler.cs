using System.Collections.Generic;
using UnityEngine;

public class PlayerBombHandler : MonoBehaviour
{
    private PlayerData pData;
    private GameData gData;

    private int currentBombs = 0;

    public void Initialize(PlayerData playerData, GameData gameData)
    {
        pData = playerData;
        gData = gameData;
    }

    public void TryPlaceBomb()
    {
        if (currentBombs >= pData.maxBombs) return;

        //Posizione (allineata alla griglia) della bomba da piazzare
        Vector3 bombPos = GridUtils.AdjustPosition(transform.position, gData.fCellSize);

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

    void ReleaseBomb() { currentBombs--; }
}
