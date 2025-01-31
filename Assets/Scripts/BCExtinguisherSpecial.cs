using UnityEngine;

public class BCExtinguisherSpecial : SpecialAttack
{
    [SerializeField] private float shootDelay;
    private float shootDelayTimer;
    [SerializeField] private float initialSpeed;

    [SerializeField] private ExtinguisherProjectile cloudPrefab;
    [SerializeField] private ExtinguisherProjectile[] cloudCache;
    [SerializeField] private int cacheCapacity;
    private int cacheIndex;

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
        if (cloudCache == null)
        {
            cloudCache = new ExtinguisherProjectile[cacheCapacity];
        }
    }
    public override void ResetAttack(float angle)
    {

    }
    public override void AimAttack(Vector2 startPosition, float angle)
    {
        if (shootDelayTimer > 0)
        {
            return;
        }

        Vector2 shootDirection = Quaternion.Euler(0, 0, angle) * Vector2.right;
        ExtinguisherProjectile firedBullet = cloudCache[cacheIndex];

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
}
