using UnityEngine;
using UnityEngine.UI;

public class UIAudioSettings : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider effectsSlider;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            musicSlider.value = AudioManager.Instance.GetMusicVolume();
            effectsSlider.value = AudioManager.Instance.GetEffectsVolume();
        }

        // Listeners Slider 
        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        effectsSlider.onValueChanged.AddListener(OnEffectsChanged);
    }


    private void OnMusicChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);
    }


    private void OnEffectsChanged(float value)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.SetEffectsVolume(value);
    }


}
