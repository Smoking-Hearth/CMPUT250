using UnityEngine;

public class ShooterEnemy : MeleeEnemy
{
    [Header("Shooting")]
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

    protected override void MoveToTarget()
    {
        targetPosition.y += targetHeight;
        base.MoveToTarget();
        targetPosition.y -= targetHeight;
    }
}
