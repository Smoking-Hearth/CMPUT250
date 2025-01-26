using UnityEngine;

public interface IInteractable
{
    public Vector2 Position { get; }
    public bool Available { get; }
    public float InteractDistance { get; }
    public void Interact();
    public void Target();
    public void Untarget();
}
