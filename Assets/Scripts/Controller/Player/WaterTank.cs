using UnityEngine;
using UnityEngine.UI;

public class WaterTank
{
    private Slider waterTankBar;
    private Slider waterTankInGame;
    private int maxWater;
    private int waterLevel;

    public WaterTank(int max)
    {
        maxWater = max;
        waterLevel = max;
    }

    public bool Full
    {
        get
        {
            return waterLevel >= maxWater;
        }
    }

    public void SetTank(Slider tankHUD, Slider tankInGame)
    {
        waterTankBar = tankHUD;
        waterTankInGame = tankInGame;

        if (waterTankBar != null)
        {
            waterTankBar.maxValue = maxWater;
            waterTankBar.value = waterLevel;
        }

        if (waterTankInGame != null)
        {
            waterTankInGame.maxValue = maxWater;
            waterTankInGame.value = waterLevel;
        }
    }

    public void RefillWater(int amount)
    {
        waterLevel += amount;

        if (waterLevel - maxWater > maxWater * 0.15f)    //Refills extra when the tank would almost be refilled but not quite
        {
            waterLevel = maxWater;
        }
        UpdateWaterHUD();
    }

    public void EmptyTank()
    {
        waterLevel = 0;
        UpdateWaterHUD();
    }

    public bool CanUseWater(int amount)
    {
        if (waterLevel < amount)
        {
            return false;
        }

        UpdateWaterHUD();
        return true;
    }

    public bool UseWater(int amount)
    {
        if (CanUseWater(amount))
        {
            waterLevel -= amount;
            UpdateWaterHUD();
            return true;
        }
        return false;
    }

    private void UpdateWaterHUD()
    {
        if (waterTankBar != null)
        {
            waterTankBar.value = waterLevel;
        }

        if (waterTankInGame != null)
        {
            waterTankInGame.value = waterLevel;
        }
    }
}
