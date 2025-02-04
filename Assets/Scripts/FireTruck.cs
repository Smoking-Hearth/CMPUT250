using UnityEngine;

public class FireTruck : MonoBehaviour, IInteractable
{
    public Vector2 Position 
    {
         get { return gameObject.transform.position; }
    }

    public bool Available
    {
        get { return true; }
    }

    public float InteractDistance 
    {
        get { return gameObject.GetComponent<BoxCollider>().bounds.extents.x; }
    }

    public void StartInteract()
    {
        // GameManager.dialogSystem.Play(["I am firetruck"]);
    }

    public void StopInteract() {}

    public void Target() {}
    public void Untarget() {}
    
}
