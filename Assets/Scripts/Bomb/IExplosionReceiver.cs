using UnityEngine;

/*  Interfaccia per i player che possono essere colpiti */

public interface IExplosionReceiver
{
    void OnHitByExplosion(ExplosionData data);
}
