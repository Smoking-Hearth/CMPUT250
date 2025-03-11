using UnityEngine;

public class GasStationBoss : MeleeEnemy
{
    [SerializeField] private Bounds arenaBounds;
    private bool airborne;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    protected override void Target()
    {
        trackingTimer = continueTrackingSeconds;

        if (distance < enemyInfo.aggroRange && canMove)
        {
            MoveToTarget();
            if (distance <= enemyInfo.attackRange)
            {
                commitAttackTimer -= Time.fixedDeltaTime;

                if (commitAttackTimer <= 0)
                {
                    currentState = EnemyState.stFrontSwing;
                }
            }
            // If Target steps out of range? Reset Timer slightly, go back to targetting
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

        if (airborne)
        {
            if (!arenaBounds.Contains(enemyRigidBody.position + enemyRigidBody.linearVelocity))
            {
                if (enemyRigidBody.linearVelocityX >= 0)
                {
                    enemyRigidBody.linearVelocityX = -enemyInfo.speed * 1.5f;
                }
                else
                {
                    enemyRigidBody.linearVelocityX = enemyInfo.speed * 1.5f;
                }
            }
        }


    }

}
