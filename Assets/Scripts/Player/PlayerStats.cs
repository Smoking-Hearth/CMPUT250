using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
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

    [SerializeField] private float hurtRadius;
    private PlayerController controller;

    void Awake()
    {
        waterTank = new WaterTank(waterTankBar, waterTankInGame, maxWater);

        health = GetComponent<Health>();
        healthBar.maxValue = health.Max;
        healthBar.minValue = health.Min;
        healthBar.value = health.Current;

        controller = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        WaterRefiller.onWaterRefill += waterTank.RefillWater;
        health.onChanged += UpdateHealthBar;
        GameManager.onEnemyAttack += CheckEnemyAttack;
    }

    private void OnDisable()
    {
        WaterRefiller.onWaterRefill -= waterTank.RefillWater;
        health.onChanged -= UpdateHealthBar;
        GameManager.onEnemyAttack -= CheckEnemyAttack;
    }

    void UpdateHealthBar()
    {
        healthBar.value = health.Current;
    }

    private void CheckEnemyAttack(Vector2 position, EnemyAttackInfo attackInfo)
    {
        Vector2 direction = (Vector2)transform.position - position;
        float distance = direction.magnitude;
        if (distance < hurtRadius + attackInfo.radius)
        {
            health.Current -= attackInfo.damage;

            float closeness = 1 - distance / (attackInfo.radius + hurtRadius);
            Vector2 knockback = new Vector2(closeness * direction.normalized.x * attackInfo.knockbackPower.x, attackInfo.knockbackPower.y);
            controller.PushPlayer(knockback);
        }
    }
}
