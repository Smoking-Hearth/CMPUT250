using UnityEngine;

public class PlayerShoot
{
    private float bulletInitialSpeed = 10.0f;

    public PlayerShoot(float speed)
    {
        bulletInitialSpeed = speed;
    }

    public void Shoot(GameObject firedBullet, Vector2 direction)
    {
        var rigidBody = firedBullet.GetComponent<Rigidbody2D>();
        if (rigidBody != null)
        {
            rigidBody.linearVelocity = direction.normalized * bulletInitialSpeed;
        }
        else
        {
            Debug.LogWarning("Water bullet prefab is missing a RigidBody. May be floating");
        }

        var timer = firedBullet.GetComponent<Timer>();
        if (timer != null)
        {
            timer.timeLeft_s = 2.0f;
            timer.OnFinishCallback += (gameObject) => {
                Object.Destroy(gameObject);
            };
        }
        else
        {
            Debug.LogWarning("Water bullets will live forever. Performance problem.");
        }
    }
}
