using UnityEngine;

// NOTE: This code could be generalized a bit and work for keeping track of water.
public class Health : MonoBehaviour
{
    public float max;
    public float current;
    public float regen = float.NegativeInfinity;

    public delegate void OnDepletedCallback();
    public event OnDepletedCallback OnDepleted;

    // When this is negative the callback is only called once.
    public float depletedCooldownDelay = float.NegativeInfinity;
    private float depletedCooldown = float.NegativeInfinity; 

    void Update()
    {
        if (depletedCooldown <= 0.0) 
        {
            if (OnDepleted != null && current <= float.Epsilon)
            {
                OnDepleted();
                depletedCooldown = depletedCooldownDelay;
            }
        } 
        else if (!float.IsNegative(depletedCooldownDelay))
        {
            depletedCooldown -= Time.deltaTime;
        }
        if (regen > 0.0 && current < max) 
        {
            current += regen * Time.deltaTime;
        }
        // NOTE: Other systems may arbitrarily modify health. 
        current = Mathf.Clamp(current, 0.0f, max);
    }
}
