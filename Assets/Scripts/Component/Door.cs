using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Interactable openInteractable;
    [SerializeField] private Interactable closeInteractable;

    public void Open()
    {
        openInteractable.StartInteract();
    }

    public void Close()
    {
        closeInteractable.StartInteract();
    }
}
