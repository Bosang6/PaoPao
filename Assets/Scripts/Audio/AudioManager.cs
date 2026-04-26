using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource effectsSource;

    [Header("Music Clip")]
    [SerializeField] private AudioClip menuMusic;


    [Header("Effects Clip")]
    [SerializeField] private AudioClip buttonSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;


    private float musicVolume = 1f;
    private float effectsVolume = 1f;



    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }


    /* Music */

    public void PlayMenuMusic()
    {
        if (musicSource == null || menuMusic == null) return;

        if(musicSource.clip == menuMusic && musicSource.isPlaying) return;

        musicSource.clip = menuMusic;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

    
    /* Effects */

    public void PlayButtonSound()
    {
        PlayEffect(buttonSound);
    }

    public void PlayWinSound()
    {
        PlayEffect(winSound);
    }

    public void PlayLoseSound()
    {
        PlayEffect(loseSound);
    }


    /* Utility */
    private void PlayEffect(AudioClip clip)
    {
        if(effectsSource == null || clip == null) return;
        effectsSource.PlayOneShot(clip, effectsVolume);
    }


    /* Volume Control */
    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetEffectsVolume(float value)
    {
        effectsVolume = Mathf.Clamp01(value);
    }

    public float GetMusicVolume()
    {
        return musicVolume;
    }

    public float GetEffectsVolume()
    {
        return effectsVolume;
    }

}
