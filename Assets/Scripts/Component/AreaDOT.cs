using UnityEngine;

public class AreaDOT : MonoBehaviour
{
    [SerializeField] private bool isEnemy;
    [SerializeField] private CombustibleKind damageClass;

    [SerializeField] private LayerMask hitLayers;
    [SerializeField] private int maxTickCount;
    [SerializeField] private float secondsBetweenTicks;
    private float tickTimer;
    private int damageCounter;

    [SerializeField] private Vector2 areaOffset;
    [SerializeField] private float areaRadius;
    [SerializeField] private float damage;

    [SerializeField] private ParticleSystem particles;

    private void OnEnable()
    {
        damageCounter = 0;
        tickTimer = 0;
        particles.Clear();
    }

    void FixedUpdate()
    {
        if (damageCounter >= maxTickCount)
        {
            return;
        }
        if (tickTimer > 0)
        {
            tickTimer -= Time.fixedDeltaTime;
        }
        else
        {
            tickTimer = secondsBetweenTicks;
            DamageTick();
        }
    }

    public void DamageTick()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll((Vector2)transform.position + areaOffset, areaRadius, hitLayers);

        for (int i = 0; i < targets.Length; i++)
        {
            if (isEnemy)
            {

            }
            else
            {
                targets[i].GetComponent<IExtinguishable>().Extinguish(damageClass, damage);
            }
        }
        damageCounter++;

        if (damageCounter >= maxTickCount)
        {
            particles.Stop();
        }
    }
}
