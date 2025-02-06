using UnityEngine;

public class ShooterEnemy : EnemyController
{
    [SerializeField] private float standRange;
    [SerializeField] private EnemyProjectile projectilePrefab;
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
        Vector2 direction = targetPosition - (Vector2)transform.position;
        if (direction.magnitude < standRange)
        {
            return;
        }
        transform.position = (Vector2)transform.position + direction.normalized * Time.fixedDeltaTime * speed;
        if (direction.x < 0)
        {
            flipTransform.localScale = new Vector2(-1, 1);
        }
        else
        {
            flipTransform.localScale = Vector2.one;
        }
    }

    protected override void Attack()
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;

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
            flipTransform.localScale = new Vector2(-1, 1);
        }
        else
        {
            flipTransform.localScale = Vector2.one;
        }

        currentIndex = (currentIndex + 1) % cacheCapacity;
        commitAttackTimer = commitAttackSeconds;
        currentState = EnemyState.stBackSwing;
    }
}
