using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    private PlayerData pData;
    private GameData gData;

    private bool isInitialized = false;
    private bool bIsMoving = false;
    private Vector3 v3TargetPosition;
    private Vector2 v2LastDirection = Vector2.zero;
    private Vector2 boxSize;
    private Animator animator;

    [SerializeField]
    private E_Animator eAnimator;

    void Start()
    {
        animator = GetComponent<Animator>(); 
        animator.runtimeAnimatorController = AnimatorManager.Instance.LoadAnimator(eAnimator);
        PlayerManager.Instance.Register(transform); 
    }

    public void Initialize(PlayerData playerData, GameData gameData)
    {
        pData = playerData;
        gData = gameData;

        //Allinea la posizione alla griglia
        transform.position = GridUtils.AdjustPosition(pData.spawnPosition, gData.fCellSize);
        transform.rotation = pData.spawnRotation;
        v3TargetPosition = transform.position;

        //Usato per il boxCast: proietta un box della stessa dimensione della cella
        boxSize = new Vector2(gData.fCellSize * 0.9f, gData.fCellSize * 0.9f); // 0.9 = margine minimo

        isInitialized = true;
    }

    void Update() { if (bIsMoving) { Move(); } }

    public void HandleInput(Vector2 input)
    {
        if (bIsMoving) return;

        float h = Mathf.RoundToInt(input.x);  //Per avere solo -1, 0 o 1
        float v = Mathf.RoundToInt(input.y);  //Per avere solo -1, 0 o 1

        Vector2 direction = Vector2.zero;

        if (h != 0) { direction = new Vector2(h, 0); }
        if (v != 0) { direction = new Vector2(0, v); }
        //Premendo due assi insieme, da priorità al verticale

        if (direction != Vector2.zero) { TryMove(direction); }
        else
        {
            if (animator != null)
            {
                // animator.SetFloat("MoveX", 0);
                // animator.SetFloat("MoveY", 0);
                animator.SetBool("IsMoving", false);
            }
        }
    }

    void TryMove(Vector2 direction)
    {
        Vector3 target = transform.position + new Vector3(direction.x, direction.y, 0) * gData.fCellSize;

        if(animator != null)
        {
            animator.SetInteger("MovingDir", DirToInt(direction));
            // animator.SetFloat("MoveX", direction.x);
            // animator.SetFloat("MoveY", direction.y);
        }

        Collider2D hit = Physics2D.OverlapBox(target, boxSize, 0f, pData.lmCollisionLayer);

        if (hit == null)    //Cella libera, ci si può muovere
        {
            v3TargetPosition = target;
            v2LastDirection = direction;
            bIsMoving = true;
            if(animator != null) { animator.SetBool("IsMoving", true); }
        }
        else                //Cella occupata, ci si ferma
        {
            v2LastDirection = Vector2.zero;
        }
    }

    void Move()
    {
        Vector3 start = transform.position;
        Vector3 end = v3TargetPosition;
        float movement = pData.moveSpeed * Time.deltaTime;

        transform.position = Vector3.MoveTowards(start, end, movement);

        if (transform.position == end)  //Invocato al termine del movimento (arrivato a destinazione)
        {
            transform.position = GridUtils.AdjustPosition(end, gData.fCellSize);   //Doppio controllo sull'accuratezza della posizione
            bIsMoving = false;
            if (animator != null) { animator.SetBool("IsMoving", false); }

            //Todo: invocare solo in mappa di ghiaccio (?)
            if (true)
            {
                CheckIcePlate();    //Controlla se si può proseguire col movimento
            }
        }

        
    }

    void CheckIcePlate()
    {
        if (v2LastDirection == Vector2.zero) return;
        
        //Controlla se la cella corrente è una IcePlate, in caso prosegue il movimento ("scivola")
        Collider2D hit = Physics2D.OverlapBox(transform.position, boxSize, 0f, gData.lmIcePlate);

        if (hit != null) { TryMove(v2LastDirection); } else { v2LastDirection = Vector2.zero; }
    }

    public void Respawn() {
        transform.position = GridUtils.AdjustPosition(pData.spawnPosition, gData.fCellSize); 
        transform.rotation = pData.spawnRotation;
        v3TargetPosition = transform.position;
        bIsMoving = false;
    }

    void OnDestroy() { PlayerManager.Instance?.Unregister(transform); }

    void OnDrawGizmos()
    {
        if(!isInitialized) { return; }

        // Mostra la posizione snappata (pallino giallo)
        Gizmos.color = Color.yellow;
        Vector3 snapped = GridUtils.AdjustPosition(transform.position, gData.fCellSize);
        Gizmos.DrawWireSphere(snapped, 0.1f);

        // Mostra il target attuale (pallino rosso)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(v3TargetPosition, 0.2f);

        // Mostra il box di rilevamento collisioni sul target (cubo verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(v3TargetPosition, new Vector3(gData.fCellSize * 0.9f, gData.fCellSize * 0.9f, 0));
    }

    int lastDir = -1;
    private int DirToInt(Vector2 direction)
    {
        if (direction == Vector2.zero) return lastDir;
        
        int x = (int) direction.x;
        int y = (int) direction.y;

        // Up
        if (x == 0 && y == 1)
        {
            lastDir = 0;
            return lastDir;
        }
        // Right
        if (x == 1 && y == 0)
        {
            lastDir = 1;
            return lastDir;
        }
        // Down
        if (x == 0 && y == -1)
        {
            lastDir = 2;
            return lastDir;
        }
        // Left
        if (x == -1 && y == 0)
        {
            lastDir = 3;
            return lastDir;
        }
        
        return lastDir;
    }

    public void SetAnimatorHurtingTrigger()
    {
        animator.SetTrigger("Hurting");
    }

    public void SetAnimatorIsDead()
    {
        animator.SetBool("IsDead", true);
    }
}
