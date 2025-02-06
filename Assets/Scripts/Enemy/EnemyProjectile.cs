using UnityEngine;

public class EnemyProjectile : Projectile
{
    [SerializeField] protected EnemyAttackInfo attackInfo;
    protected override void HitEffect(Collider2D hitTarget)
    {
        GameManager.onEnemyAttack(transform.position, transform.position, attackInfo);
    }
}
