using UnityEngine;

public class FinalBossArm : MonoBehaviour
{
    [SerializeField] private EnemyAttackInfo attackInfo;
    [SerializeField] private float attackInterval;
    [SerializeField] private Vector2 attackPosition;
    private float attackTimer;

    void FixedUpdate()
    {
        //THIS IS TEST CODE
        if (attackTimer > 0)
        {
            attackTimer -= Time.fixedDeltaTime;
        }
        else
        {
            if (GameManager.onEnemyAttack != null)
            {
                GameManager.onEnemyAttack((Vector2)transform.position + attackPosition, transform.position, attackInfo);
                attackTimer = attackInterval;
            }
        }
    }
}
