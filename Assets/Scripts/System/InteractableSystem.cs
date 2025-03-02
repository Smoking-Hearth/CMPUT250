using UnityEngine;
using System.Collections.Generic;

public class InteractableSystem: MonoBehaviour
{
    private static Interactable targetInteractable;
    public static Interactable Target
    {
        get
        {
            return targetInteractable;
        }
    }
    private static List<Interactable> interactables;

    public static void AddInteractable(Interactable interactable)
    {
        if (interactables == null)
        {
            interactables = new List<Interactable>();
        }
        if (!interactables.Contains(interactable))
        {
            interactables.Add(interactable);
        }
    }

    public static void RemoveInteractable(Interactable interactable)
    {
        if (interactables == null)
        {
            interactables = new List<Interactable>();
        }
        if (interactables.Contains(interactable))
        {
            interactables.Remove(interactable);
        }
    }

    void Update()
    {
        Vector2 playerPosition = gameObject.MyLevelManager().Player.Position;

        if (interactables == null)
        {
            return;
        }
        Interactable newTarget = null;
        float closestDistance = 1000;
        for (int i = 0; i < interactables.Count; i++)
        {
            float distance = Vector2.Distance(playerPosition, interactables[i].transform.position);
            if (distance < closestDistance && distance < interactables[i].InteractDistance)
            {
                newTarget = interactables[i];
                closestDistance = distance;
            }
        }

        if (newTarget != targetInteractable)
        {
            if (targetInteractable != null)
            {
                targetInteractable.StopInteract();
                targetInteractable.Untarget();
            }

            if (newTarget != null)
            {
                newTarget.Target();
            }

            targetInteractable = newTarget;
        }
    }

    public static void Interact(bool holding)
    {
        if (targetInteractable == null)
        {
            return;
        }

        if (holding)
        {
            targetInteractable.HoldInteract();
        }
        else
        {
            targetInteractable.StartInteract();
        }
    }

    public static void StopInteract()
    {
        if (targetInteractable == null)
        {
            return;
        }

        targetInteractable.StopInteract();
    }
}
