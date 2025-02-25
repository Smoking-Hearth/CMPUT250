using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private GameObject hydropack;

    private WaterTank waterTank;
    [SerializeField] private Slider waterTankBar;
    [SerializeField] private Slider waterTankInGame;
    [SerializeField] private Slider extinguisherTankBar;
    [SerializeField] private Slider pressureBar;
    [SerializeField] private Projectile bullet;
    [SerializeField] private float bulletInitialSpeed = 10.0f;
    [SerializeField] private float shootCooldown;
    private float shootCooldownTimer;
    [SerializeField] private float pressurizeSeconds;
    [SerializeField] private float pressureReleasePerSecond;
    [SerializeField] private float pressurizeTimer;
    [SerializeField] private SpecialAttack defaultSpecialAttack;
    [SerializeField] private float specialCooldown;
    private float specialCooldownTimer;
    [SerializeField] private AudioSource refillAudio;
    [SerializeField] private AudioClip fullAudio;

    public bool ShootAvailable
    {
        get
        {
            if (pressurizeTimer > 0 && waterTank.CanUseWater(bullet.Cost))
            {
                pressurizeTimer -= pressureReleasePerSecond * Time.fixedDeltaTime;

                if (pressurizeTimer < 0)
                {
                    pressurizeTimer = 0;
                }
            }
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
    private Projectile[] bulletCache;
    private int bulletCounter;

    [SerializeField] private int costDelayTicks;
    private int costTicks;

    [SerializeField] private Transform attachPoint;
    [SerializeField] private Image[] inventoryIcons;
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
        SpecialAttack attack = Instantiate(defaultSpecialAttack, transform).GetComponent<SpecialAttack>();
        attack.ResourceTank.SetTank(waterTankBar, waterTankInGame, refillAudio, fullAudio);
        attack.ResourceTank.EmptyTank();
        waterTank = attack.ResourceTank;
        inventory = new PlayerInventory(2, attack, attachPoint, inventoryIcons);

        bulletCache = new Projectile[maxBullets];
    }

    private void OnEnable()
    {
        SpecialAttack.onPickupSpecial += PickUpSpecial;
        SpecialAttack.onClearSpecial += DropSpecial;
        PlayerController.Controls.Hydropack.SwapSpecial.performed += inventory.Swap;
        WaterRefiller.onWaterRefill += waterTank.RefillWater;
        PlayerMovement.onLand += PlayerLand;
    }

    private void OnDisable()
    {
        SpecialAttack.onPickupSpecial -= PickUpSpecial;
        SpecialAttack.onClearSpecial -= DropSpecial;
        PlayerController.Controls.Hydropack.SwapSpecial.performed -= inventory.Swap;
        WaterRefiller.onWaterRefill -= waterTank.RefillWater;
        PlayerMovement.onLand -= PlayerLand;
    }

    private void FixedUpdate()
    {
        if (shootCooldownTimer > 0)
        {
            shootCooldownTimer -= Time.fixedDeltaTime;
        }

        if (specialCooldownTimer > 0)
        {
            specialCooldownTimer -= Time.fixedDeltaTime;
        }

        if (pressurizeTimer < pressurizeSeconds)
        {
            pressurizeTimer += Time.fixedDeltaTime;
            if (pressurizeTimer > pressurizeSeconds)
            {
                pressurizeTimer = pressurizeSeconds;
            }
        }

        pressureBar.value = pressurizeTimer / pressurizeSeconds;
    }

    public void EnableShooting()
    {
        inventory.SetVisibility(true);
        hydropack.SetActive(true);
        waterTankBar.gameObject.SetActive(true);
        pressureBar.gameObject.SetActive(true);
    }

    public void ResetAimedSprites()
    {
        for (int i = 0; i < swingObjects.Length; i++)
        {
            swingObjects[i].objectTransform.rotation = Quaternion.identity;
        }
    }

    public void AimSprites()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        //For every object that needs to point towards the mouse
        for (int i = 0; i < swingObjects.Length; i++)
        {
            Vector2 spriteDirection = mousePosition - (Vector2)swingObjects[i].objectTransform.position;
            float spriteAngle = Mathf.Rad2Deg * Mathf.Atan2(spriteDirection.y, spriteDirection.x);
            if (spriteAngle <= 90 && spriteAngle > -90)
            {
                flipObject.localScale = Vector3.one;
                float constrictAngle = spriteAngle + 90;

                if (constrictAngle > swingObjects[i].maxAngle)
                {
                    spriteAngle = swingObjects[i].maxAngle - 90;
                }
                else if (constrictAngle < swingObjects[i].minAngle)
                {
                    spriteAngle = swingObjects[i].minAngle - 90;
                }
                swingObjects[i].objectTransform.rotation = Quaternion.Euler(0, 0, spriteAngle + swingObjects[i].offsetAngle);

                if (i == swingObjects.Length - 1)
                {
                    aimAngle = spriteAngle + swingObjects[i].offsetAngle;
                }
            }
            else
            {
                flipObject.localScale = new Vector3(-1, 1, 1);
                float constrictAngle = -spriteAngle - 90;
                if (constrictAngle < 0)
                {
                    constrictAngle = 360 + constrictAngle;
                }

                if (constrictAngle > swingObjects[i].maxAngle)
                {
                    spriteAngle = -(swingObjects[i].maxAngle) - 90;
                }
                else if (constrictAngle < swingObjects[i].minAngle)
                {
                    spriteAngle = -(swingObjects[i].minAngle) - 90;
                }

                swingObjects[i].objectTransform.rotation = Quaternion.Euler(0, 0, spriteAngle + 180 - swingObjects[i].offsetAngle);

                if (i == swingObjects.Length - 1)
                {
                    aimAngle = spriteAngle - swingObjects[i].offsetAngle;
                }
            }
        }
    }

    public bool Shoot(Vector2 targetPosition)
    {
        if (!waterTank.UseWater(bullet.Cost))
        {
            return false;
        }

        Vector2 shootDirection = Quaternion.Euler(0, 0, aimAngle) * Vector2.right;
        Projectile firedBullet = bulletCache[bulletCounter];

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
        shootCooldownTimer = Mathf.Lerp(shootCooldown, 0.05f, pressurizeTimer / pressurizeSeconds);

        bulletCounter = (bulletCounter + 1) % maxBullets;
        return true;
    }

    public bool SpecialShoot(bool active)
    {
        SpecialAttack specialAttack = inventory.CurrentSpecial;
        if (active && !inventory.CurrentSpecial.ResourceTank.UseWater(specialAttack.InitialCost))
        {
            return false;
        }

        specialAttack.Activate(nozzle.position, active, transform);

        if (active)
        {
            AimSprites();
            specialAttack.ResetAttack(aimAngle);
            specialCooldownTimer = specialCooldown;
            gameObject.MyLevelManager().MusicSystem.SpecialVolume();
        }
        else
        {
            gameObject.MyLevelManager().MusicSystem.DefaultVolume();
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
            if (!inventory.CurrentSpecial.ResourceTank.UseWater(specialAttack.MaintainCost))
            {
                ResetAimedSprites();
                return false;
            }
        }
        specialAttack.AimAttack(nozzle.position, aimAngle);
        return true;
    }

    private void ResetSpecialCooldown()
    {
        specialCooldownTimer = 0;
    }
    private void PlayerLand(Vector2 landPosition, float force)
    {
        ResetSpecialCooldown();
    }

    private void PickUpSpecial(SpecialAttack special)
    {
        inventory.PickUp(special);
        if (!extinguisherTankBar.gameObject.activeSelf)
        {
            extinguisherTankBar.gameObject.SetActive(true);
        }

        switch(special.ExtinguishClass)
        {
            case CombustibleKind.A_COMMON:
                special.ResourceTank.SetTank(waterTankBar, waterTankInGame);
                break;
            default:
                special.ResourceTank.SetTank(extinguisherTankBar, null);
                break;
        }
    }

    private void DropSpecial(SpecialAttack special)
    {
        inventory.ClearCurrentSpecial();
        special.ResourceTank.SetTank(null, null);
    }
}
