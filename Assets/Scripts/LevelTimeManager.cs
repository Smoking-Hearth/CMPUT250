using UnityEngine;
using UnityEngine.UI;

public class LevelTimeManager
{
    private float timeLimitSeconds;
    private float remainingSeconds;
    private Slider progressBar;

    public delegate void OnTimeout();
    public event OnTimeout onTimeout;

    public LevelTimeManager(float limit, Slider bar)
    {
        timeLimitSeconds = limit;
        remainingSeconds = timeLimitSeconds;
        progressBar = bar;
        progressBar.maxValue = timeLimitSeconds;
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (remainingSeconds < 0)
        {
            remainingSeconds = 0;
        }
        progressBar.value = timeLimitSeconds - remainingSeconds;
    }

    public void DepleteTime(float seconds)
    {
        remainingSeconds -= seconds;
        UpdateBar();

        if (remainingSeconds <= 0 && onTimeout != null)
        {
            onTimeout();
        }
    }
}
