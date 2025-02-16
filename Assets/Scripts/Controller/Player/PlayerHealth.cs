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
    [SerializeField] private float invulnerableDuration;
    private float invulnerableTimer;
    [SerializeField] private SpriteRenderer[] blinkRenderers;
    [SerializeField] private int blinkFrequency;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color blinkColor;

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

    private void FixedUpdate()
    {
        if (invulnerableTimer > 0)
        {
            invulnerableTimer -= Time.fixedDeltaTime;
            if (invulnerableTimer <= 0)
            {
                InvulnerabilityBlink(1);
            }
            else
            {
                float time = (Mathf.Cos(invulnerableTimer / invulnerableDuration * blinkFrequency * Mathf.PI * 2) + 1) * 0.5f;
                Debug.Log(time);
                InvulnerabilityBlink(time);
            }
        }
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

    private void InvulnerabilityBlink(float time)
    {
        for (int i = 0; i < blinkRenderers.Length; i++)
        {
            blinkRenderers[i].color = Color.Lerp(blinkColor, normalColor, time);
        }
    }

    private void CheckEnemyAttack(Vector2 position, Vector2 sourcePosition, EnemyAttackInfo attackInfo)
    {
        if (gameObject.MyLevelManager().levelState != LevelState.Playing)
        {
            return;
        }

        if (invulnerableTimer > 0)
        {
            return;
        }

        Vector2 attackDirection = (Vector2)transform.position - position;
        float attackDistance = attackDirection.magnitude;

        Vector2 directionFromSource = (Vector2)transform.position - sourcePosition;
        if (attackDistance < hurtRadius + attackInfo.radius)
        {
            Current -= attackInfo.damage;
            invulnerableTimer = invulnerableDuration;

            //Play hurt sound depending on damage type
            gameObject.MyLevelManager().Player.Sounds.PlayHurt(attackInfo.damageType);

            //Player knockback
            float closeness = Mathf.Clamp01(1 - attackDistance / (attackInfo.radius + hurtRadius));
            Vector2 knockback = new Vector2(closeness * directionFromSource.normalized.x * attackInfo.knockbackPower.x, attackInfo.knockbackPower.y);
            gameObject.MyLevelManager().Player.Movement.PushPlayer(knockback);
        }
    }
}
