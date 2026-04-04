using TMPro;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private PlayerData playerData;

    [Header("Game Settings")]
    [SerializeField] private GameData gameData;

    public Vector2 gridOffset = new Vector2(0.5f, 0.5f);    //Offset dal centro della tile

    private bool bIsMoving = false;
    private Vector3 v3TargetPosition;
    private Animator animator;
    public float fCellSize;                            //Dimensione di ogni cella
    private float fSpeed = 5f;                               //Velocità di spostamento tra le celle

    private InputSystem_Actions _inputActions;
    private Vector2 _currentInput;

    private void Awake() { _inputActions = new InputSystem_Actions(); }
    void OnEnable() { _inputActions.Player.Enable(); }
    void OnDisable() { _inputActions.Player.Disable(); }
    void OnDestroy() { _inputActions.Dispose(); PlayerManager.Instance?.Unregister(transform); }


    void Start()
    {
        fSpeed = playerData.moveSpeed;
        fCellSize = gameData.fCellSize;
        transform.position = GridUtils.AdjustPosition(transform.position, fCellSize);    //Allinea la posizione alla griglia
        v3TargetPosition = transform.position;
        animator = GetComponent<Animator>();
        PlayerManager.Instance.Register(transform);
    }

    void Update() { if (bIsMoving) { Move(); } else { ReadInput(); } }


    void ReadInput()
    {
        Vector2 raw = _inputActions.Player.Move.ReadValue<Vector2>();
        float h = Mathf.RoundToInt(raw.x);  //Per avere solo -1, 0 o 1
        float v = Mathf.RoundToInt(raw.y);  //Per avere solo -1, 0 o 1

        Vector2 direction = Vector2.zero;

        if (h != 0) { direction = new Vector2(h, 0); }
        if (v != 0) { direction = new Vector2(0, v); }
        //Premendo due assi insieme, da priorità al verticale

        if (direction != Vector2.zero) { TryMove(direction); }
        else
        {
            if (animator != null)
            {
                animator.SetFloat("MoveX", 0);
                animator.SetFloat("MoveY", 0);
                animator.SetBool("IsMoving", false);
            }
        }
    }

    void TryMove(Vector2 direction)
    {
        Vector3 target = transform.position + new Vector3(direction.x, direction.y, 0) * fCellSize;

        if(animator != null)
        {
            animator.SetFloat("MoveX", direction.x);
            animator.SetFloat("MoveY", direction.y);
        }

        // BoxCast: proietta un box della stessa dimensione della cella
        Vector2 boxSize = new Vector2(fCellSize * 0.9f, fCellSize * 0.9f); // 0.9 = margine minimo
        Collider2D hit = Physics2D.OverlapBox(target, boxSize, 0f, playerData.lmCollisionLayer);

        if (hit == null)    //Cella libera, ci si può muovere
        {
            v3TargetPosition = target;
            bIsMoving = true;
            if(animator != null) { animator.SetBool("IsMoving", true); }
        }
    }

    void Move()
    {
        Vector3 start = transform.position;
        Vector3 end = v3TargetPosition;
        float movement = fSpeed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(start, end, movement);

        if (transform.position == end)
        {
            transform.position = GridUtils.AdjustPosition(end, fCellSize);   //Doppio controllo sull'accuratezza della posizione
            bIsMoving = false;
            if (animator != null) { animator.SetBool("IsMoving", false); }
        }
    }

    void OnDrawGizmos()
    {
        // Mostra la posizione snappata (pallino giallo)
        Gizmos.color = Color.yellow;
        Vector3 snapped = GridUtils.AdjustPosition(transform.position, fCellSize);
        Gizmos.DrawWireSphere(snapped, 0.1f);

        // Mostra il target attuale (pallino rosso)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(v3TargetPosition, 0.2f);

        // Mostra il box di rilevamento collisioni sul target (cubo verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(v3TargetPosition, new Vector3(fCellSize * 0.9f, fCellSize * 0.9f, 0));
    }
}
