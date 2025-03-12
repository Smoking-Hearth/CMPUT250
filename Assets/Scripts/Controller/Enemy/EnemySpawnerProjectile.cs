using UnityEngine;

public class EnemySpawnerProjectile : EnemyProjectile
{
    [Header("Spawner")]
    [SerializeField] private Transform spawnObject;

    public override void ResetProjectile()
    {
        base.ResetProjectile();
        spawnObject.gameObject.SetActive(false);
    }

    protected override void OnHit()
    {
        base.OnHit();
        spawnObject.gameObject.SetActive(true);
    }
}
