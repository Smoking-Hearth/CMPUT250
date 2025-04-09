using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using LitMotion;

public enum DamageType
{
    Fire, Electricity
}
public class PlayerHealth : Health
{
    [SerializeField] private AnimatedBar healthBar;
    [SerializeField] private float hurtRadius;
    [SerializeField] private float invulnerableDuration;
    private float invulnerableTimer;
    [SerializeField] private SpriteRenderer[] blinkRenderers;
    [SerializeField] private int blinkFrequency;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color blinkColor;
    [SerializeField] private SpriteRenderer zapEffect;
    [Min(0.01f)]
    [SerializeField] private float zapDurationSeconds = 0.1f;
    private float zapTimer;
    [SerializeField] private AnimationCurve temperatureToDamage;
    [SerializeField] private Image heatVignette;
    [Min(0.01f)]
    [SerializeField] private float temperatureHurtThreshold;
    [SerializeField] private float maxTemperature;
    [SerializeField] private float coolDelaySeconds;
    [SerializeField] private float coolRate;
    private float coolTimer;
    private float playerTemperature;

    [SerializeField] private BraindamageEffect braindamageEffect;

    public delegate void OnDeath();
    public static event OnDeath onDeath;

    public override float Current
    {
        get { return current; }
        set
        {
            if (value != current)
            {
                current = Mathf.Clamp(value, 0f, max);
                if (shouldTriggerCallback && onChanged != null)
                {
                    onChanged();
                }
            }
        }
    }

    void Awake()
    {
        healthBar.Max = Max;
        healthBar.Min = Min;
        healthBar.Current = Current;
        braindamageEffect.Awake();
    }

    private void OnEnable()
    {
        onChanged += UpdateHealthBar;
        GameManager.onEnemyAttack += CheckEnemyAttack;
        LevelManager.onPlayerRespawn += RespawnInvulnerability;
    }

    private void OnDisable()
    {
        onChanged -= UpdateHealthBar;
        GameManager.onEnemyAttack -= CheckEnemyAttack;
        LevelManager.onPlayerRespawn -= RespawnInvulnerability;
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
                InvulnerabilityBlink(time);
            }
        }

        if (zapTimer > 0)
        {
            zapTimer -= Time.fixedDeltaTime;
            zapEffect.color = Color.Lerp(new Color(1, 1, 1, 0), Color.white, zapTimer / zapDurationSeconds * 1.2f);
        }

        braindamageEffect?.FixedUpdate();

        if (playerTemperature > 0)
        {
            if (coolTimer <= 0)
            {
                playerTemperature = Mathf.Clamp(playerTemperature - coolRate * Time.fixedDeltaTime, 0, maxTemperature);
            }
            else
            {
                coolTimer -= Time.fixedDeltaTime;
            }

            heatVignette.color = new Color(1, 1, 1, playerTemperature / temperatureHurtThreshold);
        }
        else
        {
            heatVignette.gameObject.SetActive(false);
        }
    }

    private void RespawnInvulnerability()
    {
        invulnerableTimer = invulnerableDuration * 10;
    }

    private void UpdateHealthBar()
    {
        healthBar.Current = Current;

        if (gameObject.MyLevelManager().levelState != LevelState.Playing)
        {
            return;
        }

        if (healthBar.Current <= 0f || transform.position.y <= -40f)
        {
            if (onDeath != null)
            {
                onDeath();
            }
        }
    }

    private void InvulnerabilityBlink(float time)
    {
        for (int i = 0; i < blinkRenderers.Length; i++)
        {
            blinkRenderers[i].color = Color.Lerp(blinkColor, normalColor, time);
        }
    }

    public void FireDamage(float addedTemperature)
    {
        if (invulnerableTimer > 0)
        {
            return;
        }
        if (playerTemperature == 0)
        {
            heatVignette.gameObject.SetActive(true);
        }
        coolTimer = coolDelaySeconds;
        playerTemperature = Mathf.Clamp(playerTemperature + addedTemperature, 0, maxTemperature);

        if (playerTemperature >= temperatureHurtThreshold)
        {
            Current -= temperatureToDamage.Evaluate(playerTemperature);
            invulnerableTimer = invulnerableDuration;

            gameObject.MyLevelManager().Player.Sounds.PlayHurt(DamageType.Fire);
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
            braindamageEffect?.Hurt(attackInfo.damage);
            invulnerableTimer = invulnerableDuration;

            //Play hurt sound depending on damage type
            gameObject.MyLevelManager().Player.Sounds.PlayHurt(attackInfo.damageType);

            //Player knockback
            float closeness = Mathf.Clamp01(1 - attackDistance / (attackInfo.radius + hurtRadius));
            Vector2 knockback = new(closeness * directionFromSource.normalized.x * attackInfo.knockbackPower.x, attackInfo.knockbackPower.y);
            gameObject.MyLevelManager().Player.Movement.PushPlayer(knockback);

            switch (attackInfo.damageType)
            {
                case DamageType.Electricity:
                    float attackAngle = Mathf.Atan2(-directionFromSource.y, -directionFromSource.x);
                    zapTimer = zapDurationSeconds;
                    zapEffect.color = Color.white;
                    zapEffect.size = new Vector2(directionFromSource.magnitude, 1);
                    zapEffect.transform.position = transform.position;
                    zapEffect.transform.localRotation = Quaternion.Euler(0, 0, attackAngle * Mathf.Rad2Deg);
                    break;
                default:
                    break;
            }
        }
    }
}

[System.Serializable]
class BraindamageEffect 
{
    [SerializeField] private Volume volume;
    [SerializeField] private float decayAmount;
    [SerializeField] private float maxEffectAtDamage;
    
    private FilmGrain filmGrain;
    private LensDistortion lensDistortion;
    private ColorAdjustments colorAdjustments;
    private DepthOfField depthOfField;
    private ChromaticAberration chromaticAberration;

    private float accumulatedDamage = 0f;

    public void Awake()
    {
        if (volume == null) return;
        volume.profile.TryGet(out filmGrain);
        volume.profile.TryGet(out lensDistortion);
        volume.profile.TryGet(out colorAdjustments);
        if (volume.profile.TryGet(out depthOfField))
        {
            depthOfField.mode = new DepthOfFieldModeParameter(DepthOfFieldMode.Gaussian);
        }
        volume.profile.TryGet(out chromaticAberration);
    }

    public void Hurt(float damage)
    {
        accumulatedDamage += damage;
    }

    public void FixedUpdate()
    {
        if (accumulatedDamage <= 1e-6)
        {
            accumulatedDamage = 0f;
            volume.enabled = false;
            return;
        }

        volume.enabled = true;

        float x = Mathf.Clamp01(accumulatedDamage / maxEffectAtDamage);

        filmGrain.intensity.value = EaseUtility.InOutSine(x);

        lensDistortion.intensity.value = EaseUtility.InOutSine(x) * 0.34f;

        colorAdjustments.postExposure.value = EaseUtility.InOutSine(x) * 1.3f;
        colorAdjustments.contrast.value = EaseUtility.InOutSine(x) * 40f;
        colorAdjustments.saturation.value = EaseUtility.InOutSine(x) * 40f;

        chromaticAberration.intensity.value = EaseUtility.InOutSine(x);

        accumulatedDamage -= decayAmount * Time.fixedDeltaTime;
    }
}