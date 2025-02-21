using UnityEngine;

public class MeleeEnemy : EnemyController
{
    [SerializeField] private Rigidbody2D enemyRigidBody;
    [SerializeField] private float acceleration;

    protected override void Target()
    {
        targetPosition = gameObject.MyLevelManager().Player.Position;

        if (distance < enemyInfo.aggroRange && canMove)
        {
            MoveToTarget();
            if (distance <= enemyInfo.attackRange)
            {
                if (commitAttackTimer > 0)
                {
                    commitAttackTimer -= Time.fixedDeltaTime;
                }
                else if (enemyRigidBody.linearVelocityX == 0)
                {
                    currentState = EnemyState.stFrontSwing;
                }
            }
            else
            {
                commitAttackTimer = enemyInfo.commitAttackSeconds / 2f;
                currentState = EnemyState.stTargeting;
            }
        }
        else
        {
            currentState = EnemyState.stWaiting;
        }
    }
    protected override void MoveToTarget()
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;

        if (Mathf.Abs(direction.x) <= enemyInfo.standRange)
        {
            enemyRigidBody.linearVelocityX *= 0.9f;

            if (Mathf.Abs(enemyRigidBody.linearVelocityX) < acceleration)
            {
                enemyRigidBody.linearVelocityX = 0;
            }
            return;
        }
        if (direction.x > 0)
        {
            body.localScale = Vector2.one;

            if (enemyRigidBody.linearVelocityX < enemyInfo.speed)
            {
                enemyRigidBody.linearVelocityX += acceleration;
            }
            else
            {
                enemyRigidBody.linearVelocityX = enemyInfo.speed;
            }
        }
        else if (direction.x < 0)
        {
            body.localScale = new Vector2(-1, 1);
            if (enemyRigidBody.linearVelocityX > -enemyInfo.speed)
            {
                enemyRigidBody.linearVelocityX -= acceleration;
            }
            else
            {
                enemyRigidBody.linearVelocityX = -enemyInfo.speed;
            }
        }
    }
}
