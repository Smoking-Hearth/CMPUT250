using UnityEngine;

public class ShooterEnemy : EnemyController
{
    [Header("Shooting")]
    [SerializeField] private float standRange;
    [SerializeField] private float targetHeight;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private float inaccuracy;
    [SerializeField] private float shootStartDistance;
    [SerializeField] private float shootSpeed;
    [SerializeField] private int cacheCapacity;
    private EnemyProjectile[] cache;
    private int currentIndex;

    private void Start()
    {
        cache = new EnemyProjectile[cacheCapacity];
    }
    protected override void MoveToTarget()
    {
        Vector2 direction = targetPosition - (Vector2)transform.position + Vector2.up * targetHeight;
        if (direction.magnitude < standRange)
        {
            return;
        }
        transform.position = (Vector2)transform.position + direction.normalized * Time.fixedDeltaTime * enemyInfo.speed;
        if (direction.x < 0)
        {
            body.localScale = new Vector2(-1, 1);
        }
        else
        {
            body.localScale = Vector2.one;
        }
    }

    protected override void Attack()
    {
        Vector2 randomDirection = new Vector2(Random.Range(-inaccuracy, inaccuracy), Random.Range(-inaccuracy, inaccuracy));
        Vector2 direction = targetPosition - (Vector2)transform.position + randomDirection;

        if (cache[currentIndex] == null)
        {
            cache[currentIndex] = Instantiate(projectilePrefab);
        }

        EnemyProjectile currentBullet = cache[currentIndex];

        currentBullet.ResetProjectile();
        currentBullet.transform.position = (Vector2)transform.position + direction.normalized * shootStartDistance;
        currentBullet.Propel(direction.normalized * shootSpeed);

        if (direction.x < 0)
        {
            body.localScale = new Vector2(-1, 1);
        }
        else
        {
            body.localScale = Vector2.one;
        }

        currentIndex = (currentIndex + 1) % cacheCapacity;
        commitAttackTimer = enemyInfo.commitAttackSeconds;
        currentState = EnemyState.stBackSwing;
    }
}
