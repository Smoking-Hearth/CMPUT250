using UnityEngine;

public class EnemyController : MonoBehaviour 
{
    static int waterLayer;

    void Awake() 
    {
        waterLayer = LayerMask.NameToLayer("Water");
        GetComponent<Health>().OnDepleted += () => {
            Destroy(gameObject);
        };
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (waterLayer != col.gameObject.layer) return;

        var damage = col.gameObject.GetComponent<Damage>();
        if (damage == null) return;

        // Time to get hurt.
        var health = GetComponent<Health>();

        if (health == null) 
        {
            Debug.LogWarning("There exists an invulnerable enemy");
        } 
        else 
        {
            health.current -= damage.damage;
        }
        Destroy(col.gameObject);
    }
}