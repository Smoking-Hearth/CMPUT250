using UnityEngine;
using UnityEngine.UI;

public class WaterTank
{
    private Slider waterTankBar;
    private Slider waterTankInGame;
    private int maxWater;
    private int waterLevel;

    public WaterTank(Slider bar, Slider inGame, int max)
    {
        waterTankBar = bar;
        waterTankInGame = inGame;
        maxWater = max;
        waterLevel = max;

        waterTankBar.maxValue = maxWater;
        waterTankBar.value = waterLevel;

        waterTankInGame.maxValue = maxWater;
        waterTankInGame.value = waterLevel;
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
        waterTankBar.value = waterLevel;
        waterTankInGame.value = waterLevel;
    }
}
