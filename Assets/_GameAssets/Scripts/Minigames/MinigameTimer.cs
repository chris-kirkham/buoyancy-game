using System;
using TMPro;
using UnityEngine;

public class MinigameTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float time;

    private float currentTime;
    private bool isRunning;

    public event Action onTimeUp;

    private void LateUpdate()
    {
        if(isRunning)
        {
            currentTime -= Time.deltaTime;

            if(currentTime <= 0f)
            {
                OnTimeUp();
            }
        }
    }

    private void OnTimeUp()
    {
        onTimeUp?.Invoke();
    }

    public void SetTimerRunning(bool running)
    {
        isRunning = running;
    }

    public void SetTotalTime(float time)
    {
        this.time = time;
    }

    public void ResetTime()
    {
        currentTime = time;
    }
}
