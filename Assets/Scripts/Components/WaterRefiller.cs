using UnityEngine;

public class WaterRefiller : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform interactPopup;
    [SerializeField] private int waterPerRefill;
    [SerializeField] private int refillDelayTicks;
    [SerializeField] private bool available;
    [SerializeField] private float interactDistance;
    [SerializeField] private int interactResetRate;
    private int refillDelayCounter;
    private bool interacting;
    public Vector2 Position
    {
        get
        {
            return transform.position;
        }
    }
    public bool Available
    {
        get
        {
            return available;
        }
    }
    public float InteractDistance
    {
        get
        {
            return interactDistance;
        }
    }

    public delegate void OnWaterRefill(int amount);
    public static event OnWaterRefill onWaterRefill;

    public void FixedUpdate()
    {
        if (!interacting && refillDelayCounter > 0)
        {
            refillDelayCounter -= interactResetRate;

            if (refillDelayCounter < 0)
            {
                refillDelayCounter = 0;
            }
        }
    }

    public void StartInteract()
    {
        interacting = true;
    }
    public void HoldInteract()
    {
        if (!interacting)
        {
            return;
        }

        if (refillDelayCounter < refillDelayTicks)
        {
            refillDelayCounter++;
            return;
        }

        refillDelayCounter = 0;
        
        if (onWaterRefill != null)
        {
            onWaterRefill(waterPerRefill);
        }
    }
    public void StopInteract()
    {
        interacting = false;
    }
    public void Target()
    {
        interactPopup.gameObject.SetActive(true);
    }
    public void Untarget()
    {
        interactPopup.gameObject.SetActive(false);
    }
}
