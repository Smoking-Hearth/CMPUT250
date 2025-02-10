using UnityEngine;
using UnityEngine.UI;

public class LevelTimeManager: MonoBehaviour
{
    private float timeLimitSeconds;
    private float remainingSeconds;
    [SerializeField] private Slider progressBar;
    public bool activated;

    public delegate void OnTimeout();
    public event OnTimeout onTimeout;

    [SerializeField] public float levelTimeLimitSeconds;

    void Awake()
    {
        Reset(levelTimeLimitSeconds);
        UpdateBar();
        SetTimer(levelTimeLimitSeconds);
    }

    public void Reset(float limit)
    {
        timeLimitSeconds = limit;
        remainingSeconds = timeLimitSeconds;
        progressBar.maxValue = timeLimitSeconds;
    }

    // WARN: This may be unity lifecycle.
    public void Reset()
    {
        remainingSeconds = timeLimitSeconds;
    }


    public void SetTimer(float limit)
    {
        Reset(limit);
        activated = true;
    }

    public void StopTimer()
    {
        activated = false;
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

    public void SetVisibility(bool show)
    {
        progressBar.gameObject.SetActive(show);
    }

    void FixedUpdate()
    {
        if (activated && GameManager.CurrentLevel.levelState == LevelState.Playing)
        {
            DepleteTime(Time.fixedDeltaTime);
        }
    }
}
