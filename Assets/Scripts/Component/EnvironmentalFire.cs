using UnityEngine;

public class EnvironmentalFire : Fire, IExtinguishable
{
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
        Initialize(GameManager.FireSettings.GetFireInfo(fireKind));
        SetActive(true);
    }

    protected void FixedUpdate()
    {
        if (!activated)
        {
            if (lingerTimer > 0)
            {
                lingerTimer -= Time.fixedDeltaTime;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public void Extinguish(CombustibleKind extinguishClass, float quantity_L)
    {
        if ((extinguishClass & fireKind) > 0)
        {
            Temperature -= quantity_L * extinguishEffectiveness.Evaluate(Mathf.Min(temperature - minTemperature, Combustible.MAX_TEMP));
            SetLifetime(temperatureToLifetime.Evaluate(Temperature) * peakFireHeight);
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
            lingerTimer = particles.main.startLifetime.constant;
        }
    }
}
