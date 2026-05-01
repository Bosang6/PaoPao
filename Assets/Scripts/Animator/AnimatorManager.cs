using UnityEngine;

public enum E_Animator
{
    Slime1,
    Slime2,
    Slime3,
    Penguin,
    Bomberman,
}

public class AnimatorManager : MonoBehaviour
{
    public static AnimatorManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public AnimatorOverrideController LoadAnimator(E_Animator eAnimator)
    {
        AnimatorOverrideController animatorPrefab = Resources.Load<AnimatorOverrideController>(GetAnimatorPath(eAnimator));

        if (animatorPrefab != null)
        {
            return Instantiate(animatorPrefab);
        }

        return null;
    }

    private string GetAnimatorPath(E_Animator eAnimator)
    {
        string path = "";

        switch (eAnimator)
        {
            case E_Animator.Slime1:
                path = "Prefabs/Animators/Slime1";
                break;
            case E_Animator.Slime2:
                path = "Prefabs/Animators/Slime2";
                break;
            case E_Animator.Slime3:
                path = "Prefabs/Animators/Slime3";
                break;
            case E_Animator.Penguin:
                path = "Prefabs/Animators/Penguin";
                break;
            case E_Animator.Bomberman:
                path = "Prefabs/Animators/Bomberman";
                break;
        }

        return path;
    }
}
