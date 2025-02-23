using UnityEngine;

public class EnemyProjectile : Projectile
{
    [SerializeField] protected EnemyAttackInfo attackInfo;
    [SerializeField] protected FireSpreader fireSpreader;
    protected override void HitEffect(Collider2D hitTarget)
    {
        GameManager.onEnemyAttack(transform.position, transform.position, attackInfo);
    }

    protected override void OnHit()
    {
        base.OnHit();
        if (fireSpreader != null)
        {
            fireSpreader.SpreadTemperature();
        }
    }
}
