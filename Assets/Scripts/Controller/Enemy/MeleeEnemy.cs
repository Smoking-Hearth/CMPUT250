using UnityEngine;

public class MeleeEnemy : EnemyController
{
    [Header("Movement")]
    [SerializeField] protected Rigidbody2D enemyRigidBody;
    [SerializeField] protected float acceleration;
    [Range(0, 1)]
    [SerializeField] protected float deccelerationRate;
    [SerializeField] protected bool stopWhenAttacking;
    [SerializeField] protected bool moveY;      //Whether or not to affect the y velocity of the rigidbody

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (gameObject.MyLevelManager().levelState != LevelState.Playing)
        {
            enemyRigidBody.simulated = false;
        }
        else
        {
            enemyRigidBody.simulated = true;
        }
    }

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
                    if (targetPosition.x - transform.position.x > 0)
                    {
                        body.localScale = Vector2.one;
                    }
                    else
                    {
                        body.localScale = new Vector2(-1, 1);
                    }
                    currentState = EnemyState.stFrontSwing;
                }
            }
            else if (distance > enemyInfo.attackRange * 1.5f)
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

        if (Mathf.Abs(direction.magnitude) <= enemyInfo.standRange)
        {
            enemyAnimator.SetBool("IsMoving", false);
            if (moveY)
            {
                enemyRigidBody.linearVelocity *= deccelerationRate;

                if (Mathf.Abs(enemyRigidBody.linearVelocity.magnitude) < acceleration)
                {
                    enemyRigidBody.linearVelocity = Vector2.zero;
                }
            }
            else
            {
                enemyRigidBody.linearVelocityX *= deccelerationRate;

                if (Mathf.Abs(enemyRigidBody.linearVelocityX) < acceleration)
                {
                    enemyRigidBody.linearVelocityX = 0;
                }
            }
            return;
        }

        if (moveY)
        {
            if (enemyRigidBody.linearVelocity.magnitude < enemyInfo.speed)
            {
                enemyRigidBody.linearVelocity += direction.normalized * acceleration;
            }
            else
            {
                enemyRigidBody.linearVelocity = direction.normalized * enemyInfo.speed;
            }
        }
        else
        {
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

    protected override void FrontSwing()
    {
        if (!stopWhenAttacking)
        {
            MoveToTarget();
        }
        base.FrontSwing();
    }

    protected override void BackSwing()
    {
        if (!stopWhenAttacking)
        {
            MoveToTarget();
        }
        base.BackSwing();
    }
}
