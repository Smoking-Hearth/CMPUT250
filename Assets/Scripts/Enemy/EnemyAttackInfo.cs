using UnityEngine;

[System.Serializable]
public struct EnemyAttackInfo
{
    public float radius;
    public float damage;
    public Vector2 knockbackPower;

    public EnemyAttackInfo(float rad, float dmg, Vector2 knockback)
    {
        radius = rad;
        damage = dmg;
        knockbackPower = knockback;
    }
}