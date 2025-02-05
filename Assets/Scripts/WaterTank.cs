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

        if (waterLevel > maxWater)
        {
            waterLevel = maxWater;
        }
        UpdateWaterHUD();
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
