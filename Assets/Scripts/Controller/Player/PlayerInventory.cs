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
    public void SetVisibility(bool show)
    {
        currentIcon.gameObject.SetActive(show);

        if (specials[1] != null)
        {
            otherIcon.gameObject.SetActive(show);
        }
    }

    public void PickUp(SpecialAttack newSpecial)
    {
        if (currentIndex == 0)
        {
            currentIndex++;
        }
        if (specials[currentIndex] != null)
        {
            if (currentIndex == 0)
            {
                currentIndex++;
            }
            specials[currentIndex].Drop();
            ClearCurrentSpecial();
            currentIndex++;
        }
        else
        {
            otherIcon.gameObject.SetActive(true);
        }

        otherIcon.sprite = specials[0].DisplayIcon;
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

    public void ClearCurrentSpecial()
    {
        if (currentIndex == 0)
        {
            currentIndex++;
        }

        if (currentIcon != null)
        {
            currentIcon.sprite = specials[0].DisplayIcon;
        }

        if (otherIcon != null)
        {
            otherIcon.sprite = null;
        }
        specials[currentIndex] = null;

        currentIndex = 0;
    }

    public void RefillSecondary(int amount)
    {
        if (specials[1] != null)
        {
            specials[1].ResourceTank.RefillWater(amount);
        }
    }
}
