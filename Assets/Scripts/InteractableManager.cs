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
        IInteractable newTarget = null;
        float closestDistance = -1;
        for (int i = 0; i < interactables.Length; i++)
        {
            float distance = Vector2.Distance(playerPosition, interactables[i].Position);

            if (distance < closestDistance && distance < interactables[i].InteractDistance)
            {
                newTarget = interactables[i];
                closestDistance = distance;
            }
        }

        if (targetInteractable != null)
        {
            targetInteractable.Untarget();
        }

        if (newTarget !=  null)
        {
            newTarget.Target();
        }

        targetInteractable = newTarget;
    }
}
