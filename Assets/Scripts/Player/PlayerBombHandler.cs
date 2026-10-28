using System.Collections.Generic;
using UnityEngine;

public class PlayerBombHandler : MonoBehaviour
{
    public event System.Action OnBombPlaced;

    private CharacterData _characterData;
    private GameData _gameData;

    private int currentBombs = 0;
    private bool pendingBombPlacement;

    private bool IsHuman = false;

    //[AI] Ho ancora bombe a disposizione? (limite maxBombs)
    public bool HasFreeSlot => currentBombs < _characterData.maxBombs;

    //[AI] Il bot usa questa: ho una bomba libera E sulla mia cella non ce n'è già una?
    //Il secondo controllo serve perché il gioco permette di piazzare due bombe sulla stessa
    //cella (il collider della prima è spento finché il proprietario ci sta sopra), e per
    //la danger map sarebbe una complicazione inutile.
    public bool CanPlaceBomb
    {
        get
        {
            Vector3 cell = GridUtils.AdjustPosition(transform.position, _gameData.fCellSize);
            return HasFreeSlot && !BombPool.Instance.IsBombAt(cell);
        }
    }

    public void Initialize(GameData gameData, CharacterData characterData, bool IsHuman)
    {
        _gameData = gameData;
        _characterData = characterData;
        this.IsHuman = IsHuman;
    }

    public void TryPlaceBomb()
    {
        // Place only from a stable grid position; snapping a mid-step position can
        // put the bomb in the cell the player is currently moving into.
        if (_characterData.isMoving)
        {
            pendingBombPlacement = true;
            return;
        }

        PlaceBombNow();
    }

    private void Update()
    {
        if (!pendingBombPlacement || _characterData == null || _characterData.isMoving) return;
        pendingBombPlacement = false;
        PlaceBombNow();
    }

    private void PlaceBombNow()
    {
        //[AI] Comportamento invariato (per l'umano): controlla solo il limite di bombe
        if (!HasFreeSlot) return;

        //Posizione (allineata alla griglia) della bomba da piazzare
        Vector3 bombPos = GridUtils.AdjustPosition(transform.position, _gameData.fCellSize);

        BombController bomb = BombPool.Instance.Get(bombPos);
        if(bomb == null) return;

        bomb.Initialize(PlayerManager.Instance.GetAllPlayers(), IsHuman);

        // Sottoscrivi l'evento di ritorno al pool per decrementare il contatore
        bomb.OnBombReturned += ReleaseBomb;

        /*  Registra il listener "ReleaseBomb" come listener dell'evento "OnBombReturned"
         *  (che è un evento, spiegato in BombController > Explode ()). Quando, nel metodo
         *  Explode() di BombController, verrà invocato OnBombReturned?.Invoke(), Unity
         *  invocherà automaticamente la funzione ReleaseBomb() di questo player.
         *  La sintassi += permette di aprire il canale di comunicazione coi listener */

        OnBombPlaced?.Invoke();
        currentBombs++;
    }

    void ReleaseBomb() { currentBombs--; }
}
