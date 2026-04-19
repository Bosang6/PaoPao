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
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
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



}
