using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] private GameObject interactText;
    [SerializeField] private float interactDistance;
    private bool isInteracting;

    public float InteractDistance 
    {
        get
        {
            return interactDistance;
        }
    }

    [SerializeField] private UnityEvent startInteractEvent;
    [SerializeField] private UnityEvent holdInteractEvent;
    [SerializeField] private UnityEvent stopInteractEvent;

    private void OnEnable()
    {
        EnableInteractable();
    }
    private void OnDisable()
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
    public void Target()
    {
        interactText.SetActive(true);
    }
    public void Untarget()
    {
        interactText.SetActive(false);
    }
}
