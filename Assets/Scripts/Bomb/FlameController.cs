using System.Collections;
using UnityEngine;

public enum FlameType
{
    Center, 
    HorizontalLeftMid,
    HorizontalRightMid,
    HorizontalLeftEnd, 
    HorizontalRightEnd,
    VerticalTopMid, 
    VerticalBottomMid,
    VerticalTopEnd,
    VerticalBottomEnd,
}

public class FlameController : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private GameData gameData;

    private Coroutine flameCoroutine;
    private ExplosionData data;

    private Animator animator;
    private static readonly int FlameTypeHash = Animator.StringToHash("FlameType"); //Ottimizzazione

    void Awake() { animator = GetComponent<Animator>(); }

    public void Initialize(ExplosionData data, FlameType type = FlameType.Center)
    {
        if(data == null) { Debug.LogError("FlameController: ExplosionData == null"); }
        this.data = data;

        if (animator != null)
        {
            int animType = 0;
            switch (type)
            {
                case FlameType.VerticalTopEnd:
                case FlameType.VerticalBottomEnd:
                    animType = 4;
                    break;
                case FlameType.VerticalTopMid:
                case FlameType.VerticalBottomMid:
                    animType = 3;
                    break;
                case FlameType.HorizontalRightMid:
                case FlameType.HorizontalLeftMid:
                    animType = 1;
                    break;
                case FlameType.HorizontalRightEnd:
                case FlameType.HorizontalLeftEnd:
                    animType = 2;
                    break;
                    
            }
            animator.SetInteger(FlameTypeHash, animType);
        }

        if(flameCoroutine != null) { StopCoroutine(flameCoroutine); }
        flameCoroutine = StartCoroutine(Flame());

        //Se l'esplosione � verticale, ruota lo sprite
        if(type == FlameType.VerticalTopMid || type == FlameType.VerticalTopEnd) { 
            transform.rotation = Quaternion.Euler(0, 0, 90f); 
        }
        else if (type == FlameType.VerticalBottomMid || type == FlameType.VerticalBottomEnd)
        {
            transform.rotation = Quaternion.Euler(0, 0, -90f); 
        }
        else if (type == FlameType.HorizontalLeftMid || type == FlameType.HorizontalLeftEnd)
        {
            transform.rotation = Quaternion.Euler(0, 0, -180f); 
        }

        else { transform.rotation = Quaternion.identity; } //Reset per il pool
    }

    private IEnumerator Flame()
    {
        float fElapsedTime = 0f;
        float fTimeToCall = 0.1f;   //Ogni quanto controlla e chiama i receiver

        while(fElapsedTime < data.fFlameDuration)
        {
            NotifyReceivers();      //Notifica gli oggetti nella cella
            yield return new WaitForSeconds(fTimeToCall);   //Attende un tot (evita chiamata ad ogni fraem)
            fElapsedTime += fTimeToCall;

            //Nota: nel playerHealth gestisco la ricezione di pi� chiamate, altrimenti subirebbe pi� volte danno
        }

        FlamePool.Instance.ReturnToPool(this);
    }

    private void NotifyReceivers()
    {
        //Dimensione tile (leggermente ridotta per evitare di toccare tile adiacenti)
        Vector2 v2BoxSize = new Vector2(gameData.fCellSize * 0.9f, gameData.fCellSize * 0.9f);

        //Individua i player colpiti dalle fiamme
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, v2BoxSize, 0f, data.lmPlayerLayer);

        //Notifica tutti i player colpiti dalle fiamme
        foreach(Collider2D hit in hits)
        {
            IExplosionReceiver receiver = hit.GetComponent<IExplosionReceiver>();
            receiver?.OnHitByExplosion(data);
            //?. serve per evitare che vada in crash se non ci sono player che hanno subito la fiamma
        }
    }



    private void OnDisable()
    {
        if(flameCoroutine != null) { StopCoroutine(flameCoroutine); flameCoroutine = null; }
    }
}
