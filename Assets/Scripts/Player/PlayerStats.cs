using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private Slider waterTankBar;
    [SerializeField] private int maxWater;
    private int waterLevel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        waterTankBar.maxValue = maxWater;
        waterLevel = maxWater;
    }

    // Update is called once per frame
    void Update()
    {
        
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
