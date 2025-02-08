using UnityEngine;
using UnityEngine.UI;

public class LevelTimeManager
{
    private float timeLimitSeconds;
    private float remainingSeconds;
    private Slider progressBar;
    public bool activated;

    public delegate void OnTimeout();
    public event OnTimeout onTimeout;

    public LevelTimeManager(float limit, Slider bar)
    {
        progressBar = bar;
        Reset(limit);
        UpdateBar();
    }

    public void Reset(float limit)
    {
        timeLimitSeconds = limit;
        remainingSeconds = timeLimitSeconds;
        progressBar.maxValue = timeLimitSeconds;
    }

    public void Reset()
    {
        remainingSeconds = timeLimitSeconds;
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
