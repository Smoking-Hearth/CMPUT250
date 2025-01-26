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
    [SerializeField] private int maxWater;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        waterTank = new WaterTank(waterTankBar, maxWater);
    }

}
