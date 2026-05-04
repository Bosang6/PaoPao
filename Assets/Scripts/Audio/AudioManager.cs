using System.Collections.Generic;
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

    [Header("FMOD Events")]
    [SerializeField] private List<AudioEventReference> audioEvents = new();

    // Dizionario che collega ogni AudioEvent al sio EventReference FMOD
    private Dictionary<AudioEvent, EventReference> audioDictionary;
    
    private EventInstance currentMusicInstance;

    private Bus musicBus;
    private Bus sfxBus;

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
        sfxBus = RuntimeManager.GetBus("bus:/SFX");

        LoadVolumeSettings();
    }

    // Costruisce il dizionario AudioEvent -> EventReference partendo dalla lista assegnata nell'Inspector
    private void BuildAudioDictionary()
    {
        audioDictionary = new Dictionary<AudioEvent, EventReference>();

        foreach (AudioEventReference item in audioEvents)
        {
            if (item.eventReference.IsNull)
            {
                Debug.LogWarning($"AudioEvent {item.audioEvent} has no FMOD event assigned.");
                continue;
            }

            if (audioDictionary.ContainsKey(item.audioEvent))
            {
                Debug.LogWarning($"Duplicate AudioEvent found: {item.audioEvent}");
                continue;
            }

            audioDictionary.Add(item.audioEvent, item.eventReference);
        }


    }

    // Riproduce un effetto sonoro FMOD
    public void PlaySFX(AudioEvent audioEvent)
    {
        if (!TryGetEvent(audioEvent, out EventReference eventReference))
            return;

        RuntimeManager.PlayOneShot(eventReference);
    }


    // Riproduce una musica FMOD
    public void PlayMusic(AudioEvent audioEvent)
    {
        if (!TryGetEvent(audioEvent, out EventReference eventReference))
            return;

        StopMusic();

        currentMusicInstance = RuntimeManager.CreateInstance(eventReference);
        currentMusicInstance.start();
    }


    public void StopMusic()
    {
        currentMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currentMusicInstance.release();
        currentMusicInstance.clearHandle();
    }


    // Cerca un AudioEvent nel dizionario e restituisce il relativo EventReference
    private bool TryGetEvent(AudioEvent audioEvent, out EventReference eventReference)
    {
        if (audioDictionary == null) BuildAudioDictionary();

        if (!audioDictionary.TryGetValue(audioEvent, out eventReference))
        {
            Debug.LogWarning($"AudioEvent not found: {audioEvent}");
            return false;
        }

        return true;
    }


    /* FMOD Events */


    public void PlayMenuMusic()
    {
        PlayMusic(AudioEvent.MusicMenu);
    }


    public void PlayWinSound()
    {
        PlaySFX(AudioEvent.WinSound);

    }

    public void PlayLoseSound()
    {
        PlaySFX(AudioEvent.LoseSound);
    }




    /* Volume Settings */

    private void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        effectsVolume = PlayerPrefs.GetFloat(EffectsVolumeKey, 1f);

        musicBus.setVolume(musicVolume);
        sfxBus.setVolume(effectsVolume);
    }

    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(EffectsVolumeKey, effectsVolume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        musicBus.setVolume(musicVolume);
    }


    public void SetEffectsVolume(float value)
    {
        effectsVolume = Mathf.Clamp01(value);
        sfxBus.setVolume(effectsVolume);
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
