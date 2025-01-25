using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private ExtinguisherProjectile bullet;
    [SerializeField] private float bulletInitialSpeed = 10.0f;
    [SerializeField] private float shootCooldown;
    private float shootCooldownTimer;
    public bool ShootAvailable
    {
        get
        {
            return shootCooldownTimer <= 0;
        }
    }
    [SerializeField] private WaterStreamAnimator waterStream;

    [SerializeField] private float swingRadius;
    [SerializeField] private Vector2 swingPivotPosition;
    [SerializeField] private SwingObject[] swingObjects;
    [SerializeField] private Transform flipObject;
    [SerializeField] private Transform nozzle;
    private float aimAngle;

    [System.Serializable]
    private struct SwingObject
    {
        public Transform objectTransform;
        public float minAngle;
        public float maxAngle;
        public float offsetAngle;
    }

    private bool isSpecialShooting;

    private void FixedUpdate()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        //For every object that needs to point towards the mouse
        for (int i = 0; i < swingObjects.Length; i++) 
        {
            //Converting to an angle (need to conver radians to degrees)
            Vector2 direction = mousePosition - (Vector2)swingObjects[i].objectTransform.position;
            aimAngle = Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x);

            if (aimAngle <= 90 && aimAngle > -90)
            {
                flipObject.localScale = Vector3.one;
                float constrictAngle = aimAngle + 90;

                if (constrictAngle > swingObjects[i].maxAngle)
                {
                    aimAngle = swingObjects[i].maxAngle - 90;
                }
                else if (constrictAngle < swingObjects[i].minAngle)
                {
                    aimAngle = swingObjects[i].minAngle - 90;
                }
                swingObjects[i].objectTransform.rotation = Quaternion.Euler(0, 0, aimAngle + swingObjects[i].offsetAngle);
            }
            else
            {
                flipObject.localScale = new Vector3(-1, 1, 1);
                float constrictAngle = -aimAngle - 90;
                if (constrictAngle < 0)
                {
                    constrictAngle = 360 + constrictAngle;
                }

                if (constrictAngle > swingObjects[i].maxAngle)
                {
                    aimAngle = -(swingObjects[i].maxAngle) - 90;
                }
                else if (constrictAngle < swingObjects[i].minAngle)
                {
                    aimAngle = -(swingObjects[i].minAngle) - 90;
                }

                swingObjects[i].objectTransform.rotation = Quaternion.Euler(0, 0, aimAngle + 180 - swingObjects[i].offsetAngle);
            }
        }

        if (shootCooldownTimer > 0)
        {
            shootCooldownTimer -= Time.fixedDeltaTime;
        }
    }

    public void Shoot(Vector2 targetPosition)
    {
        Vector2 shootDirection = Quaternion.Euler(0, 0, aimAngle) * Vector2.right;
        ExtinguisherProjectile firedBullet = Instantiate(bullet, nozzle.position, Quaternion.identity);
        shootCooldownTimer = shootCooldown;

        firedBullet.Propel(shootDirection * bulletInitialSpeed);
    }

    public void SpecialShoot(bool active)
    {
        waterStream.transform.position = nozzle.position;
        waterStream.SetSpecialActive(active);

        if (active)
        {
            waterStream.ResetStream(aimAngle);
        }
    }

    public void AimStream()
    {
        waterStream.transform.position = nozzle.position;
        waterStream.Aim(aimAngle);
    }
}
