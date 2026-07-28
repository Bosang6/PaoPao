using UnityEngine;
using TMPro;

public class UITimer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI timerText;

    private float currentTime = 0;
    private bool isRunning = true;

    private void Update()
    {
        if (!isRunning) return;

        currentTime += Time.deltaTime;
        UpdateTimerText();
    }


    private void UpdateTimerText()
    {
        timerText.text = GetFormattedTime();
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        currentTime = 0f;
        UpdateTimerText();
    }

    // Timer pronto per essere passato agli altri Panel
    public float GetTime()
    {
        return currentTime;
    }


    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        return $"{minutes:00}:{seconds:00}";
    }


    // Multiplayer: Ferma il timer e lo allinea la tempo ufficiale deciso dal Master Client
    public void StopTimerAt(float officialTime)
    {
        currentTime = Mathf.Max(0f, officialTime);
        isRunning = false;

        UpdateTimerText();
    }


}
