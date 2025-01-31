using UnityEngine;
using UnityEngine.Events;

public class BCExtinguisherSpecial : SpecialAttack, IInteractable
{
    [SerializeField] private float shootDelay;
    private float shootDelayTimer;
    [SerializeField] private float initialSpeed;

    [SerializeField] private ExtinguisherProjectile cloudPrefab;
    [SerializeField] private ExtinguisherProjectile[] cloudCache;
    [Range(1, 100)]
    [SerializeField] private int cacheCapacity;
    private int cacheIndex;

    [SerializeField] private GameObject interactText;
    [SerializeField] private float interactDistance;
    private bool pickedUp;

    [SerializeField] private UnityEvent pickUpEvent;
    [SerializeField] private UnityEvent dropEvent;

    [SerializeField] private Component[] itemComponents;

    public Vector2 Position
    {
        get
        {
            return transform.position;
        }
    }
    public bool Available
    {
        get
        {
            return !pickedUp;
        }
    }
    public float InteractDistance
    {
        get
        {
            return interactDistance;
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (shootDelayTimer > 0)
        {
            shootDelayTimer -= Time.fixedDeltaTime;
        }
    }

    public override void Activate(Vector2 startPosition, bool active, Transform parent)
    {
        cloudCache = new ExtinguisherProjectile[cacheCapacity];
    }
    public override void ResetAttack(float angle)
    {

    }
    public override void AimAttack(Vector2 startPosition, float angle)
    {
        if (shootDelayTimer > 0)
        {
            return;
        }

        Vector2 shootDirection = Quaternion.Euler(0, 0, angle) * Vector2.right;
        ExtinguisherProjectile firedBullet = cloudCache[cacheIndex];

        if (firedBullet == null)
        {
            cloudCache[cacheIndex] = Instantiate(cloudPrefab, startPosition, Quaternion.identity);
            firedBullet = cloudCache[cacheIndex];
        }
        else
        {
            firedBullet.ResetProjectile();
            firedBullet.transform.position = startPosition;
            firedBullet.transform.rotation = Quaternion.identity;
        }
        firedBullet.Propel(shootDirection * initialSpeed);
        shootDelayTimer = shootDelay;
        cacheIndex = (cacheIndex + 1) % cacheCapacity;
    }

    public void StartInteract()
    {
        pickedUp = !pickedUp;

        if (pickedUp)
        {
            if (onPickupSpecial != null)
            {
                onPickupSpecial(this);
            }

            Untarget();
            pickUpEvent.Invoke();
        }
        else
        {
            dropEvent.Invoke();
        }
    }
    public void Target()
    {
        interactText.SetActive(true);
    }
    public void Untarget()
    {
        interactText.SetActive(false);
    }
}
