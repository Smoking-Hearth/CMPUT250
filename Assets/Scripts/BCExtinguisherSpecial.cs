using UnityEngine;
using UnityEngine.Events;

public class BCExtinguisherSpecial : SpecialAttack
{
    [SerializeField] private float shootDelay;
    private float shootDelayTimer;
    [SerializeField] private float initialSpeed;

    [SerializeField] private Projectile cloudPrefab;
    private Projectile[] cloudCache;
    [Range(1, 100)]
    [SerializeField] private int cacheCapacity;
    private int cacheIndex;

    private void Awake()
    {
        cloudCache = new Projectile[cacheCapacity];
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (shootDelayTimer > 0)
        {
            shootDelayTimer -= Time.fixedDeltaTime;
        }
    }

    public override void Activate(Vector2 startPosition, bool active, Transform parent)
    {
        initialPushTime = initialPushDuration;
    }
    public override void ResetAttack(float angle)
    {

    }
    public override void AimAttack(Vector2 startPosition, float angle)
    {
        Vector2 shootDirection = Quaternion.Euler(0, 0, angle) * Vector2.right;

        PushBack(shootDirection);

        if (initialPushTime > 0)
        {
            PushBack(shootDirection);
        }
        if (shootDelayTimer > 0)
        {
            return;
        }

        Projectile firedBullet = cloudCache[cacheIndex];

        if (firedBullet == null)
        {
            cloudCache[cacheIndex] = Instantiate(cloudPrefab, startPosition, Quaternion.identity);
            firedBullet = cloudCache[cacheIndex];
        }
        else
        {
            firedBullet.ResetProjectile();
            firedBullet.transform.position = startPosition;
            firedBullet.transform.rotation = Quaternion.identity;
        }
        firedBullet.Propel(shootDirection * initialSpeed);
        shootDelayTimer = shootDelay;
        cacheIndex = (cacheIndex + 1) % cacheCapacity;
    }

    public void PickUp()
    {
        if (onPickupSpecial != null)
        {
            onPickupSpecial(this);
        }
    }
}
