using System.Collections.Generic;
using UnityEngine;

public class PlayerBombHandler : MonoBehaviour
{
    private CharacterData _characterData;
    private GameData _gameData;

    private int currentBombs = 0;

    public void Initialize(GameData gameData, CharacterData characterData)
    {
        _gameData = gameData;
        _characterData = characterData;
    }

    public void TryPlaceBomb()
    {
        if (currentBombs >= _characterData.maxBombs) return;

        //Posizione (allineata alla griglia) della bomba da piazzare
        Vector3 bombPos = GridUtils.AdjustPosition(transform.position, _gameData.fCellSize);

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
