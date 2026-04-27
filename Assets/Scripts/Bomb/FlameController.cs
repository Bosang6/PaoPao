using System.Collections;
using UnityEngine;

public enum FlameType { Center, HorizontalMid, HorizontalEnd, VerticalMid, VerticalEnd }

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

        if(animator != null) { animator.SetInteger(FlameTypeHash, (int)type); }

        if(flameCoroutine != null) { StopCoroutine(flameCoroutine); }
        flameCoroutine = StartCoroutine(Flame());

        //Se l'esplosione è verticale, ruota lo sprite
        if(type == FlameType.VerticalMid || type == FlameType.VerticalEnd) { 
            transform.rotation = Quaternion.Euler(0, 0, 90f); 
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

            //Nota: nel playerHealth gestisco la ricezione di più chiamate, altrimenti subirebbe più volte danno
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
