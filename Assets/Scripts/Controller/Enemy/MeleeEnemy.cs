using UnityEngine;

public class MeleeEnemy : EnemyController
{
    [Header("Movement")]
    [SerializeField] protected Rigidbody2D enemyRigidBody;
    [SerializeField] protected float acceleration;
    [SerializeField] protected bool stopWhenAttacking;

    protected override void Target()
    {
        trackingTimer = continueTrackingSeconds;

        if (distance < enemyInfo.aggroRange && canMove)
        {
            MoveToTarget();
            if (distance <= enemyInfo.attackRange)
            {
                if (commitAttackTimer > 0)
                {
                    commitAttackTimer -= Time.fixedDeltaTime;
                }
                else if (enemyRigidBody.linearVelocityX == 0 || !stopWhenAttacking)
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
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("IsMoving", false);
            }
            currentState = EnemyState.stWaiting;
        }
    }
    protected override void MoveToTarget()
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;

        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("IsMoving", true);
        }

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
            if (enemyRigidBody.linearVelocityX > -enemyInfo.speed)
            {
                enemyRigidBody.linearVelocityX -= acceleration;
            }
            else
            {
                enemyRigidBody.linearVelocityX = -enemyInfo.speed;
            }
        }

        if (enemyRigidBody.linearVelocityX > 0)
        {
            body.localScale = Vector2.one;
        }
        else if (enemyRigidBody.linearVelocityX < 0)
        {
            body.localScale = new Vector2(-1, 1);
        }
    }
}
