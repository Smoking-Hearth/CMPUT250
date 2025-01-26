using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class ExtinguisherProjectile : MonoBehaviour
{
    [SerializeField] private ParticleSystem splashParticles;
    [SerializeField] private ParticleSystem travelParticles;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private LayerMask fireLayers;
    [SerializeField] private CombustibleKind extinguishClass;
    [SerializeField] private float effectiveness;

    [SerializeField] private float lifeTime;
    [SerializeField] private float splashRadius;
    [SerializeField] private float maxRadius;
    [SerializeField] private float radiusGrowthRate;
    [SerializeField] private int cost;
    public int Cost
    {
        get
        {
            return cost;
        }
    }

    private Rigidbody2D projectileRigidbody;
    private CircleCollider2D circleCollider;
    private float lifeTimeCounter;
    private float baseRadius;
    private bool hasSplashed;

    private void OnEnable()
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
        hasSplashed = false;
    }

    void FixedUpdate()
    {
        if (hasSplashed)
        {
            return;
        }

        float angle = Mathf.Atan2(projectileRigidbody.linearVelocityY, projectileRigidbody.linearVelocityX);
        spriteRenderer.transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
        lifeTimeCounter -= Time.fixedDeltaTime;
        if (circleCollider.radius < baseRadius)
        {
            circleCollider.radius += radiusGrowthRate * Time.fixedDeltaTime;
            if (circleCollider.radius >= maxRadius)
            {
                circleCollider.radius = maxRadius;
            }
        }

        if (lifeTimeCounter <= 0)
        {
            circleCollider.radius = baseRadius;
            OnSplash();
        }
    }

    public void ResetProjectile()
    {
        hasSplashed = false;
        lifeTimeCounter = lifeTime;
        circleCollider.radius = baseRadius;

        splashParticles.Clear();
        splashParticles.Stop();
        travelParticles.Clear();
        travelParticles.Play();

        spriteRenderer.enabled = true;
        projectileRigidbody.linearVelocity = Vector2.zero;
        projectileRigidbody.gravityScale = 1;
    }

    public void Propel(Vector2 force)
    {
        projectileRigidbody.linearVelocity = force;
    }

    private void OnSplash()
    {
        splashParticles.transform.rotation = spriteRenderer.transform.rotation;
        splashParticles.Play();
        travelParticles.Stop();

        spriteRenderer.enabled = false;
        projectileRigidbody.linearVelocity = Vector2.zero;
        projectileRigidbody.gravityScale = 0;

        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, splashRadius, fireLayers);
        for(int i = 0; i < hitTargets.Length; i++)
        {
            hitTargets[i].GetComponent<IExtinguishable>().Extinguish(extinguishClass, effectiveness);
        }

        hasSplashed = true;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger)
        {
            return;
        }
        OnSplash();
    }
}
