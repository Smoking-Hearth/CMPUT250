using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStats))]
public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private ExtinguisherProjectile bullet;
    [SerializeField] private float bulletInitialSpeed = 10.0f;
    [SerializeField] private float shootCooldown;
    private float shootCooldownTimer;
    [SerializeField] private SpecialAttack defaultSpecialAttack;
    [SerializeField] private float specialCooldown;
    private float specialCooldownTimer;

    public bool ShootAvailable
    {
        get
        {
            return shootCooldownTimer <= 0;
        }
    }
    public bool SpecialAvailable
    {
        get
        {
            return specialCooldownTimer <= 0;
        }
    }

    [SerializeField] private float swingRadius;
    [SerializeField] private Vector2 swingPivotPosition;
    [SerializeField] private SwingObject[] swingObjects;
    [SerializeField] private Transform flipObject;
    [SerializeField] private Transform nozzle;
    private float aimAngle;

    [Range(1, 100)]
    [SerializeField] private int maxBullets;
    private ExtinguisherProjectile[] bulletCache;
    private int bulletCounter;

    private PlayerStats stats;
    [SerializeField] private int costDelayTicks;
    private int costTicks;

    [SerializeField] private Transform attachPoint;
    private PlayerInventory inventory;

    [System.Serializable]
    private struct SwingObject
    {
        public Transform objectTransform;
        public float minAngle;
        public float maxAngle;
        public float offsetAngle;
    }

    private void Awake()
    {
        inventory = new PlayerInventory(2, defaultSpecialAttack, attachPoint);

        if (inventory.CurrentSpecial == null)
        {
            SpecialAttack attack = Instantiate(defaultSpecialAttack, transform).GetComponent<SpecialAttack>();
            inventory.PickUp(attack);
        }

        bulletCache = new ExtinguisherProjectile[maxBullets];
        if (stats == null)
        {
            stats = GetComponent<PlayerStats>();
        }
    }

    private void OnEnable()
    {
        SpecialAttack.onPickupSpecial += inventory.PickUp;
    }

    private void OnDisable()
    {
        SpecialAttack.onPickupSpecial -= inventory.PickUp;
    }

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

        if (specialCooldownTimer > 0)
        {
            specialCooldownTimer -= Time.fixedDeltaTime;
        }
    }

    public void Shoot(Vector2 targetPosition)
    {
        if (!stats.WaterTank.UseWater(bullet.Cost))
        {
            return;
        }

        Vector2 shootDirection = Quaternion.Euler(0, 0, aimAngle) * Vector2.right;
        ExtinguisherProjectile firedBullet = bulletCache[bulletCounter];

        if (firedBullet == null)
        {
            bulletCache[bulletCounter] = Instantiate(bullet, nozzle.position, Quaternion.identity);
            firedBullet = bulletCache[bulletCounter];
        }
        else
        {
            firedBullet.ResetProjectile();
            firedBullet.transform.position = nozzle.position;
            firedBullet.transform.rotation = Quaternion.identity;
        }

        firedBullet.Propel(shootDirection * bulletInitialSpeed);
        shootCooldownTimer = shootCooldown;

        bulletCounter = (bulletCounter + 1) % maxBullets;
    }

    public bool SpecialShoot(bool active)
    {
        SpecialAttack specialAttack = inventory.CurrentSpecial;
        if (active && !stats.WaterTank.UseWater(specialAttack.InitialCost))
        {
            return false;
        }

        specialAttack.Activate(nozzle.position, active, transform);

        if (active)
        {
            specialAttack.ResetAttack(aimAngle);
        }
        else
        {
            specialCooldownTimer = specialCooldown;
        }
        return true;
    }

    public bool AimStream()
    {
        SpecialAttack specialAttack = inventory.CurrentSpecial;
        costTicks++;
        if (costTicks == costDelayTicks)
        {
            costTicks = 0;
            if (!stats.WaterTank.UseWater(specialAttack.MaintainCost))
            {
                return false;
            }
        }
        specialAttack.AimAttack(nozzle.position, aimAngle);
        return true;
    }
}
