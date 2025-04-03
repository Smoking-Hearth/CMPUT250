using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    [Tooltip("Player projectile: the class of fires this projectile is effective against\nEnemy projectile: the class of fire this projectile is")]
    [SerializeField] protected CombustibleKind damageClass;

    [Header("On shoot")]
    [SerializeField] protected ParticleSystem onShootParticles;
    [SerializeField] private int cost;
    public int Cost
    {
        get
        {
            return cost;
        }
    }

    [Header("Travel")]
    [SerializeField] protected ParticleSystem travelParticles;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    [SerializeField] protected float lifeTime;
    protected float lifeTimeCounter;
    [SerializeField] protected AnimationCurve sizeOverLifetime;
    protected float baseRadius;

    protected Rigidbody2D projectileRigidbody;
    protected CircleCollider2D circleCollider;

    [Header("On hit")]
    [SerializeField] protected ParticleSystem onHitParticles;
    [SerializeField] protected LayerMask hitLayers;
    [SerializeField] protected float areaOfEffectRadius;
    [SerializeField] protected float effectiveness;
    [SerializeField] protected AudioSource hitAudio;
    [SerializeField] protected bool hitOnlyWhenFalling;     //When turned on, on hit will only trigger when the projectile has negative y velocity
    protected bool hasHit;
    protected float damageMultiplier;

    protected virtual void OnEnable()
    {
        if (projectileRigidbody == null)
        {
            projectileRigidbody = GetComponent<Rigidbody2D>();
        }
        if (circleCollider == null)
        {
            circleCollider = GetComponent<CircleCollider2D>();
        }

        lifeTimeCounter = lifeTime;
        baseRadius = circleCollider.radius;
        hasHit = false;
    }

    protected virtual void FixedUpdate()
    {
        if (hasHit)
        {
            return;
        }

        //Align the projectile's rotation with its velocity
        float angle = Mathf.Atan2(projectileRigidbody.linearVelocityY, projectileRigidbody.linearVelocityX);
        spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);

        //Scale the collider according to sizeOverLifetime
        circleCollider.radius = baseRadius * sizeOverLifetime.Evaluate(1 - lifeTimeCounter / lifeTime);

        //Calls OnHit when the projectile expires
        lifeTimeCounter -= Time.fixedDeltaTime;
        if (lifeTimeCounter <= 0)
        {
            circleCollider.radius = baseRadius;
            OnHit();
        }
    }

    public virtual void SetDamageMultiplier(float damage)
    {
        damageMultiplier = damage;
    }

    //Resets the projectile to a state where it can be deployed again. Use this for when you are caching projectiles.
    public virtual void ResetProjectile()
    {
        hasHit = false;
        lifeTimeCounter = lifeTime;
        circleCollider.radius = baseRadius;

        if (onHitParticles != null)
        {
            onHitParticles.Clear();
            onHitParticles.Stop();
        }

        if (onShootParticles != null)
        {
            onShootParticles.Clear();
            onShootParticles.Play();
        }

        if (travelParticles != null)
        {
            travelParticles.Clear();
            travelParticles.Play();
        }

        spriteRenderer.enabled = true;
        projectileRigidbody.linearVelocity = Vector2.zero;
        projectileRigidbody.simulated = true;
    }

    //Propels the projectile with the specified force vector
    public virtual void Propel(Vector2 force)
    {
        projectileRigidbody.linearVelocity = force;
    }

    //Describes the projectile's behaviour when it hits something
    protected virtual void OnHit()
    {
        if (onHitParticles != null)
        {
            onHitParticles.transform.rotation = spriteRenderer.transform.rotation;
            onHitParticles.Play();
        }

        if (travelParticles != null)
        {
            travelParticles.Stop();
        }

        spriteRenderer.enabled = false;
        projectileRigidbody.linearVelocity = Vector2.zero;
        projectileRigidbody.simulated = false;

        //Checks for all targets hit and passes them onto HitEffect
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, areaOfEffectRadius, hitLayers);
        for (int i = 0; i < hitTargets.Length; i++)
        {
            HitEffect(hitTargets[i]);
        }

        if (hitAudio != null)
        {
            hitAudio.Play();
        }

        hasHit = true;
    }

    //Describes the projectile's effect on a target it hits
    protected virtual void HitEffect(Collider2D hitTarget)
    {
        IExtinguishable extinguishable = null;
        hitTarget.TryGetComponent(out extinguishable);

        if (extinguishable != null)
        {
            extinguishable.Extinguish(damageClass, effectiveness * damageMultiplier);
        }
    }
    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.isTrigger && (hitLayers & (1 << collision.gameObject.layer)) == 0) || hasHit)
        {
            return;
        }
        if (hitOnlyWhenFalling && projectileRigidbody.linearVelocityY >= 0)
        {
            return;
        }
        OnHit();
    }
}
