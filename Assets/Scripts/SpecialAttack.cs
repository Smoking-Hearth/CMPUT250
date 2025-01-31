using UnityEngine;

public abstract class SpecialAttack : MonoBehaviour
{
    [SerializeField] protected CombustibleKind extinguishClass;
    [SerializeField] protected float effectiveness;
    [SerializeField] protected int maintainCost;
    [SerializeField] protected int initialCost;

    public delegate void OnPickupSpecial(SpecialAttack special);
    public static OnPickupSpecial onPickupSpecial;

    public abstract void Activate(Vector2 startPosition, bool active, Transform parent);
    public abstract void ResetAttack(float angle);
    public abstract void AimAttack(Vector2 startPosition, float angle);
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
}
