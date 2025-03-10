using UnityEngine;

public class CrucibleController : MeleeEnemy
{
    [Header("Crucible")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Vector2 startThrowPosition;
    [SerializeField] private float throwRange;
    [SerializeField] private float throwIntervalSeconds;
    [SerializeField] private float projectileHangTime;
    private float throwTimer;

    protected override void OnEnable()
    {
        base.OnEnable();
        throwTimer = throwIntervalSeconds;
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
        if (direction.magnitude < throwRange)
        {
            base.Attack();
            return;
        }

        Projectile projectile = Instantiate(projectilePrefab, (Vector2)transform.position + startThrowPosition, Quaternion.identity, null);
        float velocityX = direction.x / projectileHangTime;
        float velocityY = projectileHangTime * -Physics2D.gravity.y * 0.5f;
        projectile.Propel(new Vector2(velocityX, velocityY));
        throwTimer = throwIntervalSeconds;
        currentState = EnemyState.stBackSwing;
    }
}
