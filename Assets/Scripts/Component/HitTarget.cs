using UnityEngine;
using UnityEngine.Events;

public class HitTarget : MonoBehaviour, IExtinguishable
{
    [SerializeField] private UnityEvent triggerEvent;
    [SerializeField] private float triggerThreshold;
    [SerializeField] private CombustibleKind extinguisherRequired;
    [SerializeField] private bool retrigger;
    private float triggerProgress;

    public void Extinguish(CombustibleKind extinguishClass, float quantity_L)
    {
        if (triggerProgress >= triggerThreshold)
        {
            return;
        }
        if ((extinguisherRequired & extinguishClass) == 0)
        {
            return;
        }
        triggerProgress += quantity_L;

        if (triggerProgress >= triggerThreshold)
        {
            triggerEvent.Invoke();

            if (retrigger)
            {
                triggerProgress = 0;
            }
        }
    }
}
