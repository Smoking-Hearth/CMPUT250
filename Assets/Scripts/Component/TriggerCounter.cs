using UnityEngine;
using UnityEngine.Events;

public class TriggerCounter : MonoBehaviour
{
    private int counter;
    [SerializeField] private int requiredTicks;
    [SerializeField] private UnityEvent triggerEvent;
    [SerializeField] private bool retrigger;

    private void OnEnable()
    {
        counter = 0;
    }
    public void Tick()
    {
        counter++;
        if (counter == requiredTicks)
        {
            triggerEvent.Invoke();
            if (retrigger)
            {
                counter = 0;
            }
        }
    }
}
