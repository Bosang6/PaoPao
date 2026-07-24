using UnityEngine;
using UnityEngine.InputSystem;

public class LocalInputHandler : MonoBehaviour, IPlayerInput
{
    private bool isMoving = false;
    private InputActionAsset _actions;
    private InputAction _move;          
    private InputAction _placeBomb;
    private CharacterData _characterData;
    private HumanInstanceData _instanceData;

    public void Initialize(CharacterData characterData, PlayerInstanceData instanceData)
    {
        _characterData = characterData;
        _instanceData = instanceData as HumanInstanceData;

        if (_instanceData == null)
        {
            Debug.LogError($"[LocalInputHandler] Initialize fallito: atteso HumanInstanceData, ricevuto {instanceData?.GetType().Name}", this);
            return;
        }

        //Spiegazione 1
        _actions = Instantiate(_instanceData.inputActionAsset);          
        _move = _actions.FindAction("Player/Move");
        _placeBomb = _actions.FindAction("Player/PlaceBomb");

        //Spiegazione 2
        _actions.FindActionMap("Player").Enable();
        _actions.bindingMask = InputBinding.MaskByGroup(_instanceData.controlScheme);
    }

    private void OnDisable()
    {
        if (_actions == null) return;
        InputActionMap playerActionMap = _actions.FindActionMap("Player");
        playerActionMap?.Disable();
    }

    private void OnDestroy()
    {
        if (_actions != null)
        {
            Destroy(_actions);
            _actions = null;
        }
    }

    public Vector2 GetMoveInput() => _move.ReadValue<Vector2>();
    public bool GetBombInput() => _placeBomb.WasPressedThisFrame();

    public void HasMoved() { /*  */ }

    /*  Spiegazioni:
     *  1: Instantiate(playerData.inputActionAsset) crea un "clone" dell'actionAsset
     *  -Permette di evitare che, due player con lo stesso binding possano crearsi interferenza
     *  --Con 4 giocatori locali non dovrebbe verificarsi il problema (assegno 4 binding diversi)
     *  --Con giocatori tramite networking potrebbe verificarsi, quindi mi permette di evitarlo
     *      
     *  2: Player è il nome dell'actionMap (nome definito in InputSystem_Actions)
     *  -Questa ActionMap è la copia privata di ogni player (spiegazione 1)
     *  --Quindi si può abilitare / disabilitare senza problemi 
     *  -Una volta attivata, si filtra con solo lo schema del relativo player
     */
}