using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory
{
    private SpecialAttack[] specials;
    private int currentIndex;
    private Transform attachPoint;

    public SpecialAttack CurrentSpecial
    {
        get { return specials[currentIndex]; }
    }

    public PlayerInventory(int capacity, SpecialAttack current, Transform attach)
    {
        specials = new SpecialAttack[capacity];
        currentIndex = 0;
        specials[0] = current;
        attachPoint = attach;
    }

    public void PickUp(SpecialAttack newSpecial)
    {
        if (currentIndex + 1 < specials.Length && specials[currentIndex + 1] == null)
        {
            currentIndex++;
        }
        specials[currentIndex] = newSpecial;
        newSpecial.transform.parent = attachPoint;
        newSpecial.transform.localPosition = Vector2.zero;
    }

    public void Swap(InputAction.CallbackContext context)
    {
        int nextIndex = (currentIndex + 1) % specials.Length;

        if (specials[nextIndex] != null)
        {
            specials[currentIndex].Activate(Vector2.zero, false, null);
            currentIndex = nextIndex;
        }
    }
}
