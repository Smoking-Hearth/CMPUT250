using UnityEngine;

public class Health : MonoBehaviour, IStat
{
    public string Name 
    {
        get { return $"{typeof(Health)}"; }
    }
    public float Min 
    {
        get { return 0f; }
    }

    [SerializeField] float max;
    public float Max 
    {
        get { return max; }
    }

    [SerializeField] float current;
    public float Current 
    {
        get { return current; }
        set
        {
            current = Mathf.Clamp(value, 0f, max);
            if (shouldTriggerCallback && onChanged != null)
            {
                onChanged();
            }
        }
    }

    [SerializeField] float regenerationRate = 0f;
    public float RegenerationRate
    {
        get { return regenerationRate; }
        set 
        {
            regenerationRate = Mathf.Min(value, max);
        }
    }

    public bool HealthZero
    {
        get { return current <= float.Epsilon; }
    }

    public delegate void OnChangedCallback();
    public event OnChangedCallback onChanged;
    public bool shouldTriggerCallback = true;

    public void Update()
    {
        Current += regenerationRate * Time.deltaTime;
    }

    public void ResetHealth()
    {
        Current = max;
    }
}
