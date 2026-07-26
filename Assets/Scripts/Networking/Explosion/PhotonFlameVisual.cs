using System;
using System.Collections;
using UnityEngine;

/*
 * Gestisce esclusivamente l'aspetto grafico di una fiamma multiplayer
 *
 * La fiamma:
 * - non applica danni
 * - non possiede collider
 * - non distrugge muri
 * - viene mostrata localmente su ogni client
 * - ritorna nel pool al termine della propria durata
 */


[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public sealed class PhotonFlameVisual : MonoBehaviour
{

    private static readonly int FlameTypeHash = Animator.StringToHash("FlameType");

    private Animator flameAnimator;

    private Coroutine lifetimeCoroutine;

    private Action<PhotonFlameVisual> releaseCallback;


    private void Awake()
    {
        flameAnimator = GetComponent<Animator>();
    }


    // Prepara la fiamma grafica e avvia il timer dopo il quale verrà restituita al pool
    public void Initialize(FlameType flameType, float duration, Action<PhotonFlameVisual> onFinished)
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        releaseCallback = onFinished;

        ConfigureVisual(flameType);

        float safeDuration = Mathf.Max(0.01f, duration);

        lifetimeCoroutine = StartCoroutine(LifetimeRoutine(safeDuration));
    }



    // Imposta lo stesso parametro Animator e la stessa rotazione utilizzati dal FlameController del Single Player
    private void ConfigureVisual(FlameType flameType)
    {
        int animatorType = 0;

        switch (flameType)
        {
            case FlameType.HorizontalLeftMid:
            case FlameType.HorizontalRightMid:
                animatorType = 1;
                break;

            case FlameType.HorizontalLeftEnd:
            case FlameType.HorizontalRightEnd:
                animatorType = 2;
                break;

            case FlameType.VerticalTopMid:
            case FlameType.VerticalBottomMid:
                animatorType = 3;
                break;

            case FlameType.VerticalTopEnd:
            case FlameType.VerticalBottomEnd:
                animatorType = 4;
                break;
        }

        if (flameAnimator != null)
        {
            // Riavvia correttamente l'Animator quando la fiamma viene riutilizzata dal pool.
            flameAnimator.Rebind();
            flameAnimator.Update(0f);

            flameAnimator.SetInteger(FlameTypeHash, animatorType);
        }

        switch (flameType)
        {
            case FlameType.VerticalTopMid:
            case FlameType.VerticalTopEnd:
                transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                break;

            case FlameType.VerticalBottomMid:
            case FlameType.VerticalBottomEnd:
                transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                break;

            case FlameType.HorizontalLeftMid:
            case FlameType.HorizontalLeftEnd:
                transform.rotation = Quaternion.Euler(0f, 0f, -180f);
                break;

            default:
                transform.rotation = Quaternion.identity;
                break;
        }
    }


    // Mantiene attiva la fiamma per la stessa durata indicata nell'ExplosionData
    private IEnumerator LifetimeRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        lifetimeCoroutine = null;

        Action<PhotonFlameVisual> callback = releaseCallback;

        releaseCallback = null;

        callback?.Invoke(this);
    }


    // Interrompe correttamente la coroutine quando l'oggetto viene disattivato dal pool
    private void OnDisable()
    {
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        releaseCallback = null;
    }


}
