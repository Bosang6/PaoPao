using UnityEngine;

public enum E_Character
{
    Slime1,
    Slime2,
    Slime3,
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

    public AnimatorOverrideController LoadAnimator(E_Character eCharacter)
    {
        AnimatorOverrideController animatorPrefab = Resources.Load<AnimatorOverrideController>(GetAnimatorPath(eCharacter));

        if (animatorPrefab != null)
        {
            return Instantiate(animatorPrefab);
        }

        return null;
    }

    private string GetAnimatorPath(E_Character eCharacter)
    {
        string path = "";

        switch (eCharacter)
        {
            case E_Character.Slime1:
                path = "Prefabs/Animators/Slime1";
                break;
            case E_Character.Slime2:
                path = "Prefabs/Animators/Slime2";
                break;
            case E_Character.Slime3:
                path = "Prefabs/Animators/Slime3";
                break;
        }

        return path;
    }
}
