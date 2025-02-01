using UnityEngine;

public class InteractableManager
{
    private static IInteractable targetInteractable;
    public static IInteractable Target
    {
        get
        {
            return targetInteractable;
        }
    }

    private IInteractable[] interactables;

    public InteractableManager(IInteractable[] interacts)
    {
        interactables = interacts;
    }

    public void CheckNearestTarget(Vector2 playerPosition)
    {
        if (interactables == null)
        {
            return;
        }
        IInteractable newTarget = null;
        float closestDistance = 1000;
        for (int i = 0; i < interactables.Length; i++)
        {
            if (!interactables[i].Available)
            {
                continue;
            }

            float distance = Vector2.Distance(playerPosition, interactables[i].Position);

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

        if (!targetInteractable.Available)
        {
            targetInteractable = null;
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
