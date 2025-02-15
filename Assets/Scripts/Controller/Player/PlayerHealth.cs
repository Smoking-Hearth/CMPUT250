using UnityEngine;
using UnityEngine.UI;

public enum DamageType
{
    Fire, Electricity
}
public class PlayerHealth : Health
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private float hurtRadius;
    [SerializeField] private float invincibleDuration;
    private float invincibleTimer;

    public override float Current
    {
        get { return current; }
        set
        {
            current = Mathf.Clamp(value, 0f, max);
            if (shouldTriggerCallback && onChanged != null)
            {
                onChanged();
            }
        }
    }

    void Awake()
    {
        healthBar.maxValue = Max;
        healthBar.minValue = Min;
        healthBar.value = Current;
    }

    private void OnEnable()
    {
        onChanged += UpdateHealthBar;
        GameManager.onEnemyAttack += CheckEnemyAttack;
    }

    private void OnDisable()
    {
        onChanged -= UpdateHealthBar;
        GameManager.onEnemyAttack -= CheckEnemyAttack;
    }

    void UpdateHealthBar()
    {
        healthBar.value = Current;

        if (healthBar.value <= 0f || transform.position.y <= -40f)
        {  //PROBLEM: Doesn't respawn you when you fall off edge, but may not matter
            OnDeath();
        }
    }

    void OnDeath()
    {
        LevelManager.Active.levelState = LevelState.Defeat;
    }

    private void CheckEnemyAttack(Vector2 position, Vector2 sourcePosition, EnemyAttackInfo attackInfo)
    {
        if (gameObject.MyLevelManager().levelState != LevelState.Playing)
        {
            return;
        }

        Vector2 attackDirection = (Vector2)transform.position - position;
        float attackDistance = attackDirection.magnitude;

        Vector2 directionFromSource = (Vector2)transform.position - sourcePosition;
        if (attackDistance < hurtRadius + attackInfo.radius)
        {
            Current -= attackInfo.damage;
            //Play hurt sound depending on damage type
            gameObject.MyLevelManager().Player.Sounds.PlayHurt(attackInfo.damageType);

            //Player knockback
            float closeness = Mathf.Clamp01(1 - attackDistance / (attackInfo.radius + hurtRadius));
            Vector2 knockback = new Vector2(closeness * directionFromSource.normalized.x * attackInfo.knockbackPower.x, attackInfo.knockbackPower.y);
            gameObject.MyLevelManager().Player.Movement.PushPlayer(knockback);
        }
    }
}
