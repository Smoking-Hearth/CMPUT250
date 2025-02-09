using UnityEngine;

[System.Serializable]
public struct EnemyAttackInfo
{
    public DamageType damageType;
    public float radius;
    public float damage;
    public Vector2 knockbackPower;

    public EnemyAttackInfo(DamageType dmgType, float rad, float dmg, Vector2 knockback)
    {
        damageType = dmgType;
        radius = rad;
        damage = dmg;
        knockbackPower = knockback;
    }
}