using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RefillResource : MonoBehaviour
{
    public enum RefillType
    {
        Health, Water, Secondary
    }

    [SerializeField] private RefillType refillType;
    [SerializeField] private int refillAmount;
    private LevelManager levelManager;

    public delegate void OnSecondaryRefill(int amount);
    public static OnSecondaryRefill onSecondaryRefill;

    private void OnEnable()
    {
        levelManager = gameObject.MyLevelManager();
    }

    public void Pickup()
    {
        switch (refillType)
        {
            case RefillType.Health:
                Heal();
                break;
            case RefillType.Water:
                RefillWater();
                break;
            case RefillType.Secondary:
                RefillSecondary();
                break;
        }
        Destroy(gameObject);
    }
    private void Heal()
    {
        // If player interacts with item, give desired effect
        float playerMax = levelManager.Player.Health.Max;

        if (levelManager.Player.Health.Current >= playerMax - refillAmount)
        {
            levelManager.Player.Health.Current = playerMax;
        }
        else
        {
            levelManager.Player.Health.Current += refillAmount;  
        }
    }

    private void RefillWater()
    {
        if (WaterRefiller.onWaterRefill != null)
        {
            WaterRefiller.onWaterRefill(refillAmount);
        }
    }

    private void RefillSecondary()
    {
        if (onSecondaryRefill != null)
        {
            onSecondaryRefill(refillAmount);
        }
    }
}

