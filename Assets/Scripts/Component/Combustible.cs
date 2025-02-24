using System;
using UnityEngine;
using UnityEngine.Events;
using LitMotion;

[Flags]
public enum CombustibleKind
{
    A_COMMON = 1 << 0,
    B_LIQUID = 1 << 1,
    C_ELECTRICAL = 1 << 2,
    D_METAL = 1 << 3,
    K_COOKING = 1 << 4
}

/// <summary>
/// Attach this to GameObject that can burn. Note that this class works with temperature in Kelvin.
/// Just to remind: T_Kelvin = T_Celcius + ABSOLUTE_ZERO_CELCIUS. The class has a constant to avoid
/// needing to remember.
/// </summary>
public class Combustible : MonoBehaviour, IExtinguishable
{
    public const float ABSOLUTE_ZERO_CELCIUS = 273.15f;
    public const float MAX_TEMP = 20_000f;

    [Header("Visual")]
    [SerializeField] Fire firePrefab = null;
    Fire fire = null;
    [SerializeField] CombustibleKind fireKind = CombustibleKind.A_COMMON;

    [SerializeField] AnimationCurve temperatureToLifetime = AnimationCurve.Constant(0f, MAX_TEMP, 1f);
    [SerializeField] float maxLifetime;

    [SerializeField] AnimationCurve extinguishEffectiveness = AnimationCurve.Constant(0f, MAX_TEMP, 1f);
    [SerializeField] private UnityEvent ExtinguishEvent;
    [SerializeField] private UnityEvent FullyBurntEvent;
    [SerializeField] private float secondsUntilBurnt;
    [SerializeField] private SpriteRenderer burnRenderer;
    private MotionHandle burnAnim = MotionHandle.None;

    public bool Burning
    {
        get { return fire != null && fire.IsActivated; }
    }
    
    // This is in Kelvin because it makes things nice.
    [Header("Temperature")]
    [SerializeField] float temperature = 0f;
    public float Temperature 
    {
        get { return temperature; }    
        set 
        {
            temperature = Mathf.Clamp(value, 0f, MAX_TEMP);
        }
    }

    [SerializeField] float autoIgnitionTemperature = 373.15f;
    public float AutoIgnitionTemperature 
    {
        get { return autoIgnitionTemperature; }
        set 
        {
            autoIgnitionTemperature = Mathf.Max(value, 0f);
        }
    }

    LayerMask shouldBurn;
    [SerializeField] private LayerMask fireLayer;

    [SerializeField] private float fireSpreadRadius;
    [Tooltip("Units are Kelvin/second")]
    [SerializeField] float heatCopyRate = 1f;

    [Header("Fule")]
    [SerializeField] float fule = 8f;
    [Min(0.01f)]
    [SerializeField] float fullDampness = 300f;
    [SerializeField] float dryRate = 0.5f;
    [SerializeField] private float dampness;
    public float Fule 
    {
        get { return fule; }
        set 
        {
            fule = Mathf.Max(value, 0f);
        }
    }

    // In Kelvin per unit of fule
    [SerializeField] float fuleToTemp = 10f;

    void Awake()
    {
        if (firePrefab == null)
        {
            DevLog.Error("Missing fire prefab.");
        }

        // NOTE: Something weird was happening here. I had set shouldBurn to Player in the
        // editor but when I logged it it was Fire. So that's why it's getting set manually
        shouldBurn = LayerMask.NameToLayer("Player");
        gameObject.MyLevelManager().onActivate += Activate;
        gameObject.MyLevelManager().onDeactivate += Deactivate;
        dampness = 0;
    }

    private void Activate()
    {
        gameObject.MyLevelManager().onFireTick += CheckFireSpread;
    }
    private void Deactivate()
    {
        gameObject.MyLevelManager().onFireTick -= CheckFireSpread;
    }

    void Update()
    {
        if (!gameObject.ShouldUpdate()) return;

        bool haveFule = fule > 0f;
        if (temperature > autoIgnitionTemperature && haveFule)
        {
            Ignite();
        }
        if (Burning)
        {
            float consumed = Mathf.Min(Time.deltaTime, fule);
            if (fule > 0)
            {
                fule -= consumed;
                Temperature += fuleToTemp * consumed;

                if (fule <= 0)
                {
                    FullyBurntEvent.Invoke();
                }
            }

            fire.SetLifetime(temperatureToLifetime.Evaluate(Temperature) * maxLifetime);
        }
        if (dampness > 0)
        {
            dampness = Math.Max(dampness - dryRate * Time.deltaTime, 0);
        }
    }

    private void CheckFireSpread()
    {
        Collider2D[] fires = Physics2D.OverlapCircleAll(transform.position, fireSpreadRadius, fireLayer);
        for (int i = 0; i < fires.Length; i++)
        {
            HeatSpread(fires[i].GetComponentInParent<Combustible>());
        }
    }

    private void HeatSpread(Combustible other)
    {
        if (fule <= 0)
        {
            return;
        }
        if (other != null)
        {
            float diff = other.Temperature - temperature;
            if (diff > 0f)
            {
                Temperature += diff * heatCopyRate * (1 - dampness / fullDampness);
            }
        }

        // We shouldn't damage the player if not on fire.
        if (!Burning) return;
        float distance = Vector2.Distance(gameObject.MyLevelManager().Player.Position, (Vector2)transform.position);
        if (distance < fireSpreadRadius * 0.8f)
        {
            gameObject.MyLevelManager().Player.Health.FireDamage(20f * Mathf.Pow(0.5f, distance));
        }
    }

    public void Ignite()
    {
        if (Burning) return;
        if (fire == null) 
        {
            fire = Instantiate(firePrefab, transform);
            fire.Initialize(GameManager.FireSettings.GetFireInfo(fireKind));
            fire.transform.localPosition = Vector3.zero;
        }
        fire.SetActive(true);
        gameObject.layer = LayerMask.NameToLayer("Combusted");

        if (burnRenderer != null)
        {
            burnAnim = LMotion.Create(burnRenderer.color, new Color(0.1f, 0.1f, 0.1f), secondsUntilBurnt)
                .Bind(burnRenderer, (color, renderer) => renderer.color = color);
        }
    }

    public void Extinguish(CombustibleKind extinguishClass, float quantity_L)
    {
        if (!Burning) return;
        if ((extinguishClass & fireKind) > 0)
        {
            Temperature -= quantity_L * extinguishEffectiveness.Evaluate(Mathf.Min(temperature - autoIgnitionTemperature, MAX_TEMP));

            if (extinguishClass == CombustibleKind.A_COMMON)
            {
                dampness += quantity_L;
            }

            if (fire != null && fire.IsActivated && (temperature < autoIgnitionTemperature))
            {
                CompleteExtinguish();
            }
            else
            {
                fire.HitSound();
            }
        }
        else if (fireKind == CombustibleKind.C_ELECTRICAL)
        {
            Vector2 playerPosition = gameObject.MyLevelManager().Player.Position;
            Vector2 direction = (Vector2)transform.position - playerPosition;
            GameManager.onEnemyAttack(playerPosition + direction.normalized * 0.5f, transform.position, GameManager.FireSettings.electricBackfire);
        }
    }

    public void CompleteExtinguish()
    {
        burnAnim.TryCancel();
        fire.ExtinguishSound();
        fire.SetActive(false);
        ExtinguishEvent.Invoke();
        gameObject.layer = LayerMask.NameToLayer("Combustible"); //Not the best system, but it works
    }
}
