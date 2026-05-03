using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(StudioEventEmitter))]
public class UIButtonSound : MonoBehaviour
{
    private Button button;
    private StudioEventEmitter eventEmitter;

    private void Awake()
    {
        button = GetComponent<Button>();
        eventEmitter = GetComponent<StudioEventEmitter>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(PlaySound);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(PlaySound);
    }

    private void PlaySound()
    {
        eventEmitter.Play();
    }
}