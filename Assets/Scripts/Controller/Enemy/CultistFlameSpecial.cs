using UnityEngine;

public class CultistFlameSpecial : MonoBehaviour
{
    [SerializeField] protected int maintainCost;
    [SerializeField] protected int initialCost;

    [SerializeField] private float turnSpeed;
    [SerializeField] private float streamSpeed;
    [SerializeField] private ParticleSystem nozzleParticles;

    [SerializeField] protected float initialPushDuration;
    protected float initialPushTime;

    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip deploySound;
    [SerializeField] protected AudioClip endSound;

    [Range(1, 100)]
    [SerializeField] private int cacheCapacity;
    [SerializeField] private float shootInterval;
    [SerializeField] private Projectile flameProjectilePrefab;
    private float shootTimer;
    private Projectile[] cache;
    private int cacheIndex;

    [SerializeField] private AudioSource persistSoundSource;

    public int MaintainCost
    {
        get
        {
            return maintainCost;
        }
    }
    public int InitialCost
    {
        get
        {
            return initialCost;
        }
    }

    protected void OnEnable()
    {
        if (cache == null)
        {
            cache = new Projectile[cacheCapacity];
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (shootTimer > 0)
        {
            shootTimer -= Time.fixedDeltaTime;
        }
    }
    public void Activate(Vector2 startPosition, bool set, Transform parent)
    {
        if (set)
        {
            audioSource.PlayOneShot(deploySound);
            persistSoundSource.Play();
            nozzleParticles.Clear();
            nozzleParticles.Play();
            transform.parent = parent;
            //gameObject.MyLevelManager().Player.Movement.StartPush();
        }
        else
        {
            nozzleParticles.Stop();
            persistSoundSource.Stop();
        }

        initialPushTime = initialPushDuration;
        transform.position = startPosition;
    }

    public void AimAttack(Vector2 startPosition, float aimAngle)
    {
        if (shootTimer > 0)
        {
            return;
        }
        transform.position = startPosition;
        Vector2 targetDirection = Quaternion.Euler(0, 0, aimAngle) * Vector2.right;

        nozzleParticles.transform.rotation = Quaternion.Euler(0, 0, aimAngle);
        Shoot(targetDirection);

        //PushBack(targetDirection);
    }

    private void Shoot(Vector2 targetDirection)
    {
        Projectile projectile = null;
        if (cache[cacheIndex] == null)
        {
            projectile = Instantiate(flameProjectilePrefab, (Vector2)transform.position, Quaternion.identity, null);
            cache[cacheIndex] = projectile;
        }
        else
        {
            projectile = cache[cacheIndex];
            projectile.transform.position = (Vector2)transform.position;
        }

        cacheIndex = (cacheIndex + 1) % cacheCapacity;

        projectile.ResetProjectile();
        projectile.Propel(targetDirection.normalized * streamSpeed);

        shootTimer = shootInterval;
    }
}
