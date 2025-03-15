using UnityEngine;

public class CrucibleController : MeleeEnemy
{
    [Header("Crucible")]
    [SerializeField] private Projectile projectilePrefab;
    [Range(1, 100)]
    [SerializeField] private int cacheCapacity;
    [SerializeField] private Vector2 startThrowPosition;
    [SerializeField] private float throwRange;
    [SerializeField] private float throwIntervalSeconds;
    [SerializeField] private float projectileHangTime;
    [SerializeField] private float overshootDistance;
    private float throwTimer;
    private Projectile[] projectileCache;
    private int cacheIndex;

    protected override void OnEnable()
    {
        base.OnEnable();
        throwTimer = throwIntervalSeconds;

        if (projectileCache == null)
        {
            projectileCache = new Projectile[cacheCapacity];
        }
    }

    protected override void Target()
    {
        base.Target();
        if (distance < enemyInfo.aggroRange && canMove)
        {
            if (distance >= throwRange)
            {
                throwTimer -= Time.fixedDeltaTime;
                if (throwTimer <= 0)
                {
                    currentState = EnemyState.stFrontSwing;
                }
            }
            else
            {
                throwTimer = throwIntervalSeconds;
            }
        }
    }

    protected override void Attack()
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;
        base.Attack();
        if (direction.magnitude < throwRange)
        {
            return;
        }

        //Throws a projectile when the player is too far
        Projectile projectile = null;

        if (projectileCache[cacheIndex] == null)
        {
            projectile = Instantiate(projectilePrefab, (Vector2)transform.position + startThrowPosition, Quaternion.identity, null);
            projectileCache[cacheIndex] = projectile;
        }
        else
        {
            projectile = projectileCache[cacheIndex];
            projectile.transform.position = (Vector2)transform.position + startThrowPosition;
        }

        cacheIndex = (cacheIndex + 1) % cacheCapacity;

        float velocityX = (direction.x + overshootDistance * Mathf.Sign(direction.x)) / projectileHangTime;
        float velocityY = (direction.y - startThrowPosition.y - 0.5f * projectileHangTime * Physics2D.gravity.y * projectileHangTime) / projectileHangTime;

        projectile.ResetProjectile();
        projectile.Propel(new Vector2(velocityX, velocityY));
        throwTimer = throwIntervalSeconds;
        currentState = EnemyState.stBackSwing;
    }
}
