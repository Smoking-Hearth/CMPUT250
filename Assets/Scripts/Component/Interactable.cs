using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] protected GameObject interactText;   //Text that pops up to prompt the user to interact
    [SerializeField] protected float interactDistance;    //Max distance the player can be to interact
    [SerializeField] protected OutlineComponent outline;
    protected bool isInteracting;     //If the player is currently holding the interact button
    public float InteractDistance
    {
        get
        {
            return interactDistance;
        }
    }

    [SerializeField] protected UnityEvent startInteractEvent;
    [SerializeField] protected UnityEvent holdInteractEvent;
    [SerializeField] protected UnityEvent stopInteractEvent;

    protected virtual void OnEnable()
    {
        EnableInteractable();
    }
    protected virtual void OnDisable()
    {
        DisableInteractable();
    }

    public void EnableInteractable()
    {
        InteractableSystem.AddInteractable(this);
    }

    public void DisableInteractable()
    {
        InteractableSystem.RemoveInteractable(this);
    }

    public void StartInteract()
    {
        startInteractEvent.Invoke();
        isInteracting = true;
    }
    public void HoldInteract()
    {
        if (!isInteracting)
        {
            return;
        }
        holdInteractEvent.Invoke();
    }
    public void StopInteract()
    {
        stopInteractEvent.Invoke();
        isInteracting = false;
    }
    public virtual void Target()
    {
        interactText.SetActive(true);
        if (outline != null)
        {
            outline.enabled = true;
        }
    }
    public virtual void Untarget()
    {
        interactText.SetActive(false);
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}
