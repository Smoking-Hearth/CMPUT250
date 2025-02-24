using UnityEngine;
using UnityEngine.Events;

public class TriggerCounter : MonoBehaviour
{
    private int counter;
    [SerializeField] private int requiredTicks;
    [SerializeField] private UnityEvent triggerEvent;
    [SerializeField] private bool retrigger;

    public void Tick()
    {
        counter++;

        if (counter == requiredTicks || (retrigger && counter >= requiredTicks))
        {
            triggerEvent.Invoke();
        }
    }
}
