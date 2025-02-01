using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] protected bool isEnemyProjectile;
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
    protected bool hasHit;

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

        float angle = Mathf.Atan2(projectileRigidbody.linearVelocityY, projectileRigidbody.linearVelocityX);
        spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        lifeTimeCounter -= Time.fixedDeltaTime;
        if (circleCollider.radius < baseRadius)
        {
            circleCollider.radius = baseRadius * sizeOverLifetime.Evaluate(lifeTimeCounter / lifeTime);
        }

        if (lifeTimeCounter <= 0)
        {
            circleCollider.radius = baseRadius;
            OnHit();
        }
    }

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
        projectileRigidbody.gravityScale = 1;
    }

    public virtual void Propel(Vector2 force)
    {
        projectileRigidbody.linearVelocity = force;
    }

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
        projectileRigidbody.gravityScale = 0;

        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, areaOfEffectRadius, hitLayers);
        for (int i = 0; i < hitTargets.Length; i++)
        {
            HitEffect(hitTargets[i]);
        }

        hasHit = true;
    }

    protected virtual void HitEffect(Collider2D hitTarget)
    {
        if (isEnemyProjectile)
        {

        }
        else
        {
            hitTarget.GetComponent<IExtinguishable>().Extinguish(damageClass, effectiveness);
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger || hasHit)
        {
            return;
        }
        OnHit();
    }
}
