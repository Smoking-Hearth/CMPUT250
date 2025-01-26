using UnityEngine;

public interface IInteractable
{
    public Vector2 Position { get; }
    public bool Available { get; }
    public float InteractDistance { get; }
    public void StartInteract();
    public void HoldInteract();
    public void StopInteract();
    public void Target();
    public void Untarget();
}
