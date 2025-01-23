using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletInitialSpeed = 10.0f;
    [SerializeField] private float swingRadius;
    [SerializeField] private Vector2 swingPivotPosition;
    [SerializeField] private Transform[] aimObjects;
    [SerializeField] private Transform flipObject;
    [SerializeField] private Transform nozzle;
    private Vector2 startShootPosition;

    private void FixedUpdate()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        startShootPosition = (Vector2)transform.position + swingRadius * (mousePosition - (Vector2)transform.position + swingPivotPosition).normalized;

        //For every object that needs to point towards the mouse
        for (int i = 0; i < aimObjects.Length; i++) 
        {
            //Converting to an angle (need to conver radians to degrees)
            float aimAngle = Mathf.Rad2Deg * Mathf.Atan2(mousePosition.y - aimObjects[i].position.y, mousePosition.x - aimObjects[i].position.x);

            if (aimAngle <= 90 && aimAngle > -90)
            {
                flipObject.localScale = Vector3.one;
                aimObjects[i].rotation = Quaternion.Euler(0, 0, aimAngle);
            }
            else
            {
                flipObject.localScale = new Vector3(-1, 1, 1);
                aimObjects[i].rotation = Quaternion.Euler(0, 0, aimAngle + 180);
            }
        }
    }

    public void Shoot(Vector2 targetPosition)
    {
        Vector2 shootDirection = targetPosition - startShootPosition;
        GameObject firedBullet = Instantiate(bullet, nozzle.position, Quaternion.identity);

        var rigidBody = firedBullet.GetComponent<Rigidbody2D>();
        if (rigidBody != null)
        {
            rigidBody.linearVelocity = shootDirection.normalized * bulletInitialSpeed;
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
                Destroy(gameObject);
            };
        }
        else
        {
            Debug.LogWarning("Water bullets will live forever. Performance problem.");
        }
    }
}
