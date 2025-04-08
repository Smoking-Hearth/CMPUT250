using UnityEngine;

public class FinalBossArm : MonoBehaviour
{
    [SerializeField] private Animator summonEffect;
    [Min(1)]
    [SerializeField] private float armTemperature;
    [SerializeField] private float extendTimeSeconds;
    private float extendTimer;

    [SerializeField] private CapsuleCollider2D fireCollider;
    [SerializeField] private Vector2 initialColliderOffset;
    [SerializeField] private EnvironmentalFire fire;
    [SerializeField] private AnimationCurve temperatureToLifetime = AnimationCurve.Constant(0f, Combustible.MAX_TEMP, 1f);
    [SerializeField] private float peakFireHeight;
    [SerializeField] private float fireRadius;
    private bool activated;
    public bool IsActivated
    {
        get
        {
            return activated;
        }
    }

    private void Awake()
    {
        int randomKind = Random.Range(0, 2);

        switch(randomKind)
        {
            case 0:
                fire.Initialize(GameManager.FireSettings.GetFireInfo(CombustibleKind.A_COMMON));
                break;
            case 1:
                fire.Initialize(GameManager.FireSettings.GetFireInfo(CombustibleKind.B_LIQUID));
                break;
        }
        SetActive(true);
    }

    private void OnEnable()
    {
        fire.SetLifetime(0.1f);
    }

    void FixedUpdate()
    {
        if (!activated)
        {
            return;
        }    
        if (extendTimer > 0)
        {
            extendTimer -= Time.fixedDeltaTime;

            if (extendTimer <= 0)
            {
                fire.FireRadius = fireRadius;
                fireCollider.offset = Vector2.zero;
            }
            else if (extendTimer <= 0.1f)
            {
                fire.SetLifetime(temperatureToLifetime.Evaluate(fire.Temperature) * peakFireHeight);
            }
        }

        if (!fire.IsActivated)
        {
            summonEffect.SetTrigger("Disappear");
        }
    }

    public void SetActive(bool set)
    {
        activated = set;
        fire.SetActive(set);
        fire.Temperature = 100;

        if (set)
        {
            summonEffect.SetTrigger("Activate");
            fire.Temperature = armTemperature;
            extendTimer = extendTimeSeconds;
            fire.FireRadius = 0;
            fireCollider.offset = initialColliderOffset;
        }
    }
}
