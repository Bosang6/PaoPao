using UnityEngine;

public class PlayerBombHandler : MonoBehaviour
{
    [Header("Bomb Settings")]
    [SerializeField] private int maxBombs = 1;       // quante bombe può piazzare contemporaneamente
    private int currentBombs = 0;

    private PlayerMove movement;              // riferimento al tuo script movimento

    void Awake() { movement = GetComponent<PlayerMove>(); }

    void Update() { if (Input.GetKeyDown(KeyCode.Space)) { TryPlaceBomb(); } }

    void TryPlaceBomb()
    {
        if (currentBombs >= maxBombs) return;

        // Snap della posizione del player alla griglia
        Vector3 bombPos = SnapToGrid(transform.position);

        BombController bomb = BombPool.Instance.Get(bombPos);

        // Sottoscrivi l'evento di ritorno al pool per decrementare il contatore
        bomb.OnBombReturned += HandleBombReturned;

        currentBombs++;
    }

    void HandleBombReturned()
    {
        currentBombs--;
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        float cellSize = movement.fCellSize;
        return new Vector3(
            Mathf.Round(pos.x / cellSize) * cellSize,
            Mathf.Round(pos.y / cellSize) * cellSize,
            pos.z
        );
    }
}
