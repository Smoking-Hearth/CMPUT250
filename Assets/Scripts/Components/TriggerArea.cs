using UnityEngine;
using UnityEngine.Events;

public class TriggerArea : MonoBehaviour
{
    [SerializeField] private LayerMask triggerLayers;
    [SerializeField] private UnityEvent triggerEvent;
    [SerializeField] private bool retrigger;
    [SerializeField] private float retriggerCooldownSeconds;
    private float retriggerTimer;

    private void FixedUpdate()
    {
        if (retriggerTimer > 0)
        {
            retriggerTimer -= Time.fixedDeltaTime;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (retriggerTimer > 0)
        {
            return;
        }

        if ((triggerLayers & (1 << collision.gameObject.layer)) != 0)
        {
            triggerEvent.Invoke();
            retriggerTimer = retriggerCooldownSeconds;

            if (!retrigger)
            {
                Destroy(gameObject);
            }
        }
    }
}
