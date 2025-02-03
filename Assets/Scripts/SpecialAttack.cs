using UnityEngine;

public abstract class SpecialAttack : MonoBehaviour
{
    [SerializeField] protected CombustibleKind extinguishClass;
    [SerializeField] protected float effectiveness;
    [SerializeField] protected int maintainCost;
    [SerializeField] protected int initialCost;
    [SerializeField] protected Sprite inventoryIcon;

    public delegate void OnPickupSpecial(SpecialAttack special);
    public static OnPickupSpecial onPickupSpecial;

    [SerializeField] protected Vector2 pushbackMultiplier;
    [SerializeField] protected float pushbackInitial;
    [SerializeField] protected float pushbackAcceleration;
    [SerializeField] protected float initialPushDuration;
    protected float initialPushTime;

    public delegate void OnPushback(Vector2 acceleration);
    public static event OnPushback onPushback;

    public abstract void Activate(Vector2 startPosition, bool active, Transform parent);
    public abstract void ResetAttack(float angle);
    public abstract void AimAttack(Vector2 startPosition, float angle);
    public virtual Sprite DisplayIcon
    {
        get
        {
            return inventoryIcon;
        }
    }
    public virtual int MaintainCost
    {
        get
        {
            return maintainCost;
        }
    }
    public virtual int InitialCost
    {
        get
        {
            return initialCost;
        }
    }

    public virtual void PushBack(Vector2 targetDirection)
    {
        if (onPushback != null)
        {
            Vector2 pushDirection = new Vector2(-targetDirection.x * pushbackMultiplier.x, -targetDirection.y * pushbackMultiplier.y);

            if (initialPushTime > 0)
            {
                onPushback(pushDirection * pushbackInitial * Time.fixedDeltaTime);
                initialPushTime -= Time.fixedDeltaTime;
            }
            else
            {
                onPushback(pushDirection * pushbackAcceleration * Time.fixedDeltaTime);
            }
        }
    }
}
