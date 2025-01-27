using UnityEngine;

public class EnemyController : MonoBehaviour 
{
    static int waterLayer;
    Health health;

    void Awake() 
    {
        waterLayer = LayerMask.NameToLayer("Water");
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        health.onChanged += HealthChanged;
    }

    void OnDisable()
    {
        health.onChanged -= HealthChanged;
    }

    void HealthChanged()
    {
        if (health.HealthZero)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (waterLayer != col.gameObject.layer) return;

        var damage = col.gameObject.GetComponent<Damage>();
        if (damage == null) return;

        // Time to get hurt.
        if (health == null) 
        {
            Debug.LogWarning("There exists an invulnerable enemy");
        } 
        else 
        {
            health.Current -= damage.damage;
        }
        Destroy(col.gameObject);
    }

}