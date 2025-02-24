using UnityEngine;
using UnityEngine.Events;

public class TriggerCounter : MonoBehaviour
{
    private int counter;
    [SerializeField] private int requiredTicks;
    [SerializeField] private UnityEvent triggerEvent;

    public void Tick()
    {
        counter++;

        if (counter >= requiredTicks)
        {
            triggerEvent.Invoke();
        }
    }
}
