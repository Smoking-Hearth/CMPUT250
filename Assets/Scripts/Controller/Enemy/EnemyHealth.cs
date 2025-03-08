using UnityEngine;

public class EnemyHealth : Health, IExtinguishable
{
    private float blinkTimer;
    [SerializeField] private SpriteRenderer[] blinkRenderers;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color blinkColor;
    private ParticleSystem hurtParticles;
    [SerializeField] private Vector2 hurtParticlesOffset;
    [SerializeField] EnemyHealthBar enemyHealthBar;
    public override float Max

    {
        get { return enemyInfo.maxHealth; }
    }

    private EnemySO enemyInfo;
    public EnemySO EnemyInfo {
        get
        {
            return enemyInfo;
        }
        set
        {
            if (hurtParticles == null)
            {
                hurtParticles = Instantiate(value.hurtParticles, (Vector2)transform.position + hurtParticlesOffset, Quaternion.identity, transform);
            }
            enemyInfo = value;
        }
    }
    public delegate void OnHurt();
    public event OnHurt onHurt;

    private void OnEnable()
    {
        if (enemyHealthBar != null)
        {
            enemyHealthBar.ActivateBar();
        }
    }

    private void OnDisable()
    {
        if (enemyHealthBar != null)
        {
            enemyHealthBar.DeactivateBar();
        }
    }

    private void FixedUpdate()
    {
        if (blinkTimer > 0)
        {
            blinkTimer -= Time.fixedDeltaTime;
            if (blinkTimer <= 0)
            {
                HurtBlink(1);
            }
            else
            {
                float time = (Mathf.Cos(blinkTimer / EnemyInfo.blinkDuration * EnemyInfo.blinkFrequency * Mathf.PI * 2) + 1) * 0.5f;
                HurtBlink(time);
            }
        }
    }

    private void HurtBlink(float time)
    {
        for (int i = 0; i < blinkRenderers.Length; i++)
        {
            blinkRenderers[i].color = Color.Lerp(blinkColor, normalColor, time);
        }
    }

    public virtual void Extinguish(CombustibleKind extinguishClass, float quantity_L)
    {
        if (HealthZero)
        {
            return;
        }
        if ((extinguishClass & enemyInfo.fireKind) == 0)
        {
            if (enemyInfo.fireKind == CombustibleKind.C_ELECTRICAL)
            {
                Vector2 playerPosition = gameObject.MyLevelManager().Player.Position;
                Vector2 direction = (Vector2)transform.position - playerPosition;
                GameManager.onEnemyAttack(playerPosition + direction.normalized * 0.5f, transform.position, GameManager.FireSettings.electricBackfire);
            }
            return;
        }
        Current -= quantity_L;
        blinkTimer = EnemyInfo.blinkDuration;
        hurtParticles.Play();

        onHurt?.Invoke();

        if (enemyHealthBar != null)
        {
            enemyHealthBar.UpdateHealthBar(Current, Max);
        }
    }
}
