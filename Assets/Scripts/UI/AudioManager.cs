using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("UI Clips")]
    [SerializeField] private AudioClip audioClip;

    [Header("Sources")]
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public float PlayButtonClick()
    {
        if (audioClip == null || audioSource == null)
            return 0f;

        audioSource.PlayOneShot(audioClip);
        return audioClip.length;
    }
}