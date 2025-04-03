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
    [SerializeField] protected bool wander;
    protected float wanderDirection;
    [SerializeField] protected float maxYtarget;
    [SerializeField] protected LayerMask wallLayers;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (wander)
        {
            float distX = gameObject.MyLevelManager().Player.Position.x - transform.position.x;
            if (Mathf.Abs(distX) <= enemyInfo.standRange)
            {
                wanderDirection = (Random.Range(-1f, 1f) < 0 ? -1 : 1);
            }
            else
            {
                wanderDirection = Mathf.Sign(distX);
            }
        }
    }

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

    protected override void Idle()
    {
        float distY = gameObject.MyLevelManager().Player.Position.y - transform.position.y;
        if (distance < enemyInfo.aggroRange && canMove && (Mathf.Abs(distY) < maxYtarget || !wander))
        {
            currentState = EnemyState.stTargeting;
        }

        if (!wander)
        {
            return;
        }
        if (enemyAnimator != null)
        {
            enemyAnimator.SetBool("IsMoving", true);
        }

        if (wanderDirection > 0)
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
        else if (wanderDirection < 0)
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

        RaycastHit2D wallCheck = Physics2D.Raycast(transform.position, Vector2.right * wanderDirection, 1, wallLayers);
        if (wallCheck)
        {
            wanderDirection *= -1;
        }
    }

    protected override void Target()
    {
        trackingTimer = continueTrackingSeconds;

        float distY = gameObject.MyLevelManager().Player.Position.y - transform.position.y;
        if (distance < enemyInfo.aggroRange && canMove && (Mathf.Abs(distY) < maxYtarget || !wander))
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

            if (wander)
            {
                float distX = gameObject.MyLevelManager().Player.Position.x - transform.position.x;
                if (Mathf.Abs(distX) <= enemyInfo.standRange)
                {
                    wanderDirection = (Random.Range(-1f, 1f) < 0 ? -1 : 1);
                }
                else
                {
                    wanderDirection = Mathf.Sign(distX);
                }
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
