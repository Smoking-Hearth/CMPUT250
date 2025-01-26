using UnityEngine;
using UnityEngine.UI;

public class WaterTank
{
    private Slider waterTankBar;
    private int maxWater;
    private int waterLevel;

    public WaterTank(Slider bar, int max)
    {
        waterTankBar = bar;
        maxWater = max;
        waterLevel = max;

        waterTankBar.maxValue = maxWater;
        waterTankBar.value = waterLevel;

    }

    public void RefillWater(int amount)
    {
        waterLevel += amount;

        if (waterLevel > amount)
        {
            waterLevel = maxWater;
        }
    }

    public bool UseWater(int amount)
    {
        if (waterLevel < amount)
        {
            return false;
        }

        waterLevel -= amount;
        UpdateWaterHUD();
        return true;
    }

    private void UpdateWaterHUD()
    {
        waterTankBar.value = waterLevel;
    }
}
