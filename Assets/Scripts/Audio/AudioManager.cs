using UnityEngine;
using FMODUnity;
using FMOD.Studio;



public class AudioManager : MonoBehaviour
{

    /*
     * Questo script gestisce la musica e gli effetti sonori del gioco. 
     * Utilizza il pattern Singleton per garantire un'unica istanza durante l'intero ciclo di vita del gioco.
     * Permette di regolare il volume della musica e degli effetti, e salva queste impostazioni usando PlayerPrefs.
     * PlayerPrefs è un sistema di Unity per salvare dati semplici in locale. 
     */



    // Singleton 
    public static AudioManager Instance { get; private set; }

    [Header("Audio Source")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource effectsSource;

    [Header("Music Clip")]
    [SerializeField] private StudioEventEmitter menuMusicEmitter;

    [Header("Effects Clip")]
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    private Bus musicBus;

    private float musicVolume = 1f;
    private float effectsVolume = 1f;

    private const string MusicVolumeKey = "MusicVolume";
    private const string EffectsVolumeKey = "EffectsVolume";


    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicBus = RuntimeManager.GetBus("bus:/Music");

        LoadVolumeSettings();
    }


    /* FMOD Music */

    public void PlayMenuMusic()
    {
        if (menuMusicEmitter == null) return;

        menuMusicEmitter.Play();


    }

    public void StopMusic()
    {
        if (menuMusicEmitter == null) return;
        menuMusicEmitter.Stop();
    }

    
    /* Effects */

    //public void PlayButtonSound()
    //{
    //    PlayEffect(buttonSound);
    //}

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

    private void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        effectsVolume = PlayerPrefs.GetFloat(EffectsVolumeKey, 1f);

        musicBus.setVolume(musicVolume);
        if(effectsSource != null) effectsSource.volume = effectsVolume;
    }

    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(EffectsVolumeKey, effectsVolume);
        PlayerPrefs.Save();
    }


    /* Volume Control */

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        musicBus.setVolume(musicVolume);
    }

    public void SetEffectsVolume(float value)
    {
        effectsVolume = Mathf.Clamp01(value);
        if(effectsSource != null) effectsSource.volume = effectsVolume;
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
