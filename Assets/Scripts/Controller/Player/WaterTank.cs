using UnityEngine;
using UnityEngine.UI;


public class WaterTank
{
    private AnimatedBar waterTankBar;
    private AnimatedBar waterTankInGame;
    private AudioSource refillAudio;
    private AudioClip fullAudio;
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

    public void SetTank(AnimatedBar tankHUD, AnimatedBar tankInGame)
    {
        waterTankBar = tankHUD;
        waterTankInGame = tankInGame;

        if (waterTankBar != null)
        {
            waterTankBar.Min = 0;
            waterTankBar.Max = maxWater;
            waterTankBar.Current = waterLevel;
        }

        if (waterTankInGame != null)
        {
            waterTankInGame.Min = 0;
            waterTankInGame.Max = maxWater;
            waterTankInGame.Current = waterLevel;
        }
    }
    public void SetTank(AnimatedBar tankHUD, AnimatedBar tankInGame, AudioSource audio, AudioClip full)
    {
        waterTankBar = tankHUD;
        waterTankInGame = tankInGame;
        refillAudio = audio;
        fullAudio = full;

        if (waterTankBar != null)
        {
            waterTankBar.Min = 0;
            waterTankBar.Max = maxWater;
            waterTankBar.Current = waterLevel;
        }

        if (waterTankInGame != null)
        {
            waterTankInGame.Min = 0;
            waterTankInGame.Max = maxWater;
            waterTankInGame.Current = waterLevel;
        }
    }

    public void RefillWater(int amount)
    {
        waterLevel += amount;

        if (maxWater - waterLevel < maxWater * 0.1f)    //Refills extra when the tank would almost be refilled but not quite
        {
            waterLevel = maxWater;
        }

        if (refillAudio != null)
        {
            if (waterLevel < maxWater)
            {
                refillAudio.pitch = 0.9f + (float)waterLevel / maxWater;
                refillAudio.Play();
            }
            else
            {
                refillAudio.PlayOneShot(fullAudio);
            }
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
            waterTankBar.Current = waterLevel;
        }

        if (waterTankInGame != null)
        {
            waterTankInGame.Current = waterLevel;
        }
    }
}
