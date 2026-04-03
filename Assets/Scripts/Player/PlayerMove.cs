using TMPro;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Grid Settings")]
    public float fCellSize = 1f;                            //Dimensione di ogni cella
    public Vector2 gridOffset = new Vector2(0.5f, 0.5f);    //Offset dal centro della tile
    public float fSpeed = 5f;                               //Velocità di spostamento tra le celle
    public LayerMask lmCollisionLayer;                      //Layer degli ostacoli (muri, bombe, ...)

    private bool bIsMoving = false;
    private Vector3 v3TargetPosition;
    private Animator animator;

    void Start()
    {
        transform.position = AdjustPosition(transform.position);    //Allinea la posizione alla griglia
        v3TargetPosition = transform.position;
        animator = GetComponent<Animator>();
    }

    void Update() { if (bIsMoving) { Move(); } else { ReadInput(); } }


    void ReadInput()
    {
        //GetAxisRaw permette di rendere i valori solo -1, 0 o 1
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

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
        Collider2D hit = Physics2D.OverlapBox(target, boxSize, 0f, lmCollisionLayer);

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
            transform.position = AdjustPosition(end);   //Doppio controllo sull'accuratezza della posizione
            bIsMoving = false;
            if (animator != null) { animator.SetBool("IsMoving", false); }
        }
    }

    Vector3 AdjustPosition(Vector3 pos)
    {
        /*  A causa delle moltiplicazioni tra float, a lungo andare 
         *  la posizione potrebbe essere leggermente traslata, questa 
         *  funzione permette di aggiustare la posizione, rimettendola 
         *  in linea con la griglia */

        float newX = Mathf.Round(pos.x / fCellSize) * fCellSize;
        float newY = Mathf.Round(pos.y / fCellSize) * fCellSize;
        return new Vector3(newX, newY, pos.z);  //La z rimane invariata
    }

    void OnDrawGizmos()
    {
        // Mostra la posizione snappata (pallino giallo)
        Gizmos.color = Color.yellow;
        Vector3 snapped = AdjustPosition(transform.position);
        Gizmos.DrawWireSphere(snapped, 0.1f);

        // Mostra il target attuale (pallino rosso)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(v3TargetPosition, 0.2f);

        // Mostra il box di rilevamento collisioni sul target (cubo verde)
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(v3TargetPosition, new Vector3(fCellSize * 0.9f, fCellSize * 0.9f, 0));
    }
}
