using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerInventory
{
    private SpecialAttack[] specials;
    private int currentIndex;
    private Transform attachPoint;
    private Image currentIcon;
    private Image otherIcon;

    public SpecialAttack CurrentSpecial
    {
        get { return specials[currentIndex]; }
    }

    public PlayerInventory(int capacity, SpecialAttack current, Transform attach, Image[] icons)
    {
        specials = new SpecialAttack[capacity];
        currentIndex = 0;
        specials[0] = current;
        attachPoint = attach;
        currentIcon = icons[0];
        otherIcon = icons[1];
    }

    public void PickUp(SpecialAttack newSpecial)
    {
        if (currentIndex + 1 < specials.Length && specials[currentIndex + 1] == null)
        {
            otherIcon.sprite = specials[currentIndex].DisplayIcon;
            currentIndex++;
        }
        specials[currentIndex] = newSpecial;
        newSpecial.transform.parent = attachPoint;
        newSpecial.transform.localPosition = Vector2.zero;
        currentIcon.sprite = specials[currentIndex].DisplayIcon;
    }

    public void Swap(InputAction.CallbackContext context)
    {
        int nextIndex = (currentIndex + 1) % specials.Length;

        if (specials[nextIndex] != null)
        {
            specials[currentIndex].Activate(Vector2.zero, false, null);
            otherIcon.sprite = specials[currentIndex].DisplayIcon;
            currentIndex = nextIndex;
            currentIcon.sprite = specials[currentIndex].DisplayIcon;
        }
    }
}
