using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    private WaterTank waterTank;
    public WaterTank WaterTank
    {
        get
        {
            return waterTank;
        }
    }

    [SerializeField] private Slider waterTankBar;
    [SerializeField] private Slider waterTankInGame;
    [SerializeField] private int maxWater;

    private Health health;
    [SerializeField] private Slider healthBar;

    void Awake()
    {
        waterTank = new WaterTank(waterTankBar, waterTankInGame, maxWater);

        health = GetComponent<Health>();
        healthBar.maxValue = health.Max;
        healthBar.minValue = health.Min;
        healthBar.value = health.Current;
    }

    private void OnEnable()
    {
        WaterRefiller.onWaterRefill += waterTank.RefillWater;
        health.onChanged += HealthChanged;
    }

    private void OnDisable()
    {
        WaterRefiller.onWaterRefill -= waterTank.RefillWater;
        health.onChanged -= HealthChanged;
    }

    void HealthChanged()
    {
        healthBar.value = health.Current;
    }
}
