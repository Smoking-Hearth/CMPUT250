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

    public bool Full
    {
        get
        {
            return specials[specials.Length] != null;
        }
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
        if (currentIndex == 0)
        {
            otherIcon.sprite = specials[currentIndex].DisplayIcon;
            currentIndex++;
        }
        if (specials[currentIndex] != null)
        {
            Drop();
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

    public void Drop()
    {
        if (currentIndex == 0)
        {
            return;
        }
        specials[currentIndex].transform.parent = null;
        currentIcon.sprite = null;
        specials[currentIndex].DropEvent.Invoke();
        specials[currentIndex].transform.localScale = Vector2.one;

        currentIndex = 0;
    }
}
