using UnityEngine;

public class FinalBossArm : Fire, IExtinguishable
{
    [SerializeField] private EnemyAttackInfo attackInfo;
    [SerializeField] private float attackInterval;
    [SerializeField] private Vector2 startAttackPosition;
    [SerializeField] private Vector2 attackPosition;
    [Min(1)]
    [SerializeField] private float armTemperature;
    private float attackTimer;
    [SerializeField] private float extendTimeSeconds;
    private float extendTimer;

    [SerializeField] private CombustibleKind fireKind = CombustibleKind.A_COMMON;
    [SerializeField] private float minTemperature = 425f;
    [SerializeField] private float temperature = 500f;
    [SerializeField] private AnimationCurve extinguishEffectiveness = AnimationCurve.Constant(0f, Combustible.MAX_TEMP, 1f);
    [SerializeField] private AnimationCurve temperatureToLifetime = AnimationCurve.Constant(0f, Combustible.MAX_TEMP, 1f);
    [SerializeField] private float peakFireHeight;
    private float lingerTimer;

    public float Temperature
    {
        get { return temperature; }
        set
        {
            temperature = Mathf.Clamp(value, 0f, Combustible.MAX_TEMP);
        }
    }

    private void Awake()
    {
        int randomKind = Random.Range(0, 3);

        switch(randomKind)
        {
            case 0:
                fireKind = CombustibleKind.A_COMMON;
                break;
            case 1:
                fireKind = CombustibleKind.B_LIQUID;
                break;
            case 2:
                fireKind = CombustibleKind.C_ELECTRICAL;
                break;
        }
        Initialize(GameManager.FireSettings.GetFireInfo(fireKind));
        SetActive(true);
    }

    private void OnEnable()
    {
        SetLifetime(0.1f);
    }

    void FixedUpdate()
    {
        if (extendTimer > 0)
        {
            extendTimer -= Time.fixedDeltaTime;

            if (extendTimer <= 0)
            {
                SetLifetime(temperatureToLifetime.Evaluate(Temperature) * peakFireHeight);
            }
        }
        else if (attackTimer > 0)
        {
            attackTimer -= Time.fixedDeltaTime;
        }
        else
        {
            if (GameManager.onEnemyAttack != null)
            {
                float ratio = Temperature / armTemperature;
                Vector2 lerpPosition = Vector2.Lerp(startAttackPosition + (Vector2)transform.position, attackPosition + (Vector2)transform.position, ratio);
                GameManager.onEnemyAttack(lerpPosition, transform.position, attackInfo);
                attackTimer = attackInterval;
            }
        }

        if (!activated)
        {
            if (lingerTimer > 0)
            {
                lingerTimer -= Time.fixedDeltaTime;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    public override void SetActive(bool set)
    {
        base.SetActive(set);
        Temperature = armTemperature;
        extendTimer = extendTimeSeconds;
    }

    public void Extinguish(CombustibleKind extinguishClass, float quantity_L)
    {
        if ((extinguishClass & fireKind) > 0)
        {
            Temperature -= quantity_L * extinguishEffectiveness.Evaluate(Mathf.Min(temperature - minTemperature, Combustible.MAX_TEMP));
            if (extendTimer <= 0)
            {
                SetLifetime(temperatureToLifetime.Evaluate(Temperature) * peakFireHeight);
            }
            HitSound();
        }
        else if (fireKind == CombustibleKind.C_ELECTRICAL)
        {
            Vector2 playerPosition = gameObject.MyLevelManager().Player.Position;
            Vector2 direction = (Vector2)transform.position - playerPosition; 
            GameManager.onEnemyAttack(playerPosition + direction.normalized * 0.5f, transform.position, GameManager.FireSettings.electricBackfire);
        }

        if (temperature == 0)
        {
            ExtinguishSound();
            SetActive(false);
            lingerTimer = particles.main.startLifetime.constant * 2;
        }
    }
}
