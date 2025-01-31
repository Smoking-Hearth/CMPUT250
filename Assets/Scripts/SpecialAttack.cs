using UnityEngine;

public abstract class SpecialAttack : MonoBehaviour
{
    [SerializeField] protected CombustibleKind extinguishClass;
    [SerializeField] protected float effectiveness;

    public abstract void Activate(Vector2 startPosition, bool active, Transform parent);
    public abstract void ResetAttack(float angle);
    public abstract void AimAttack(Vector2 startPosition, float angle);
    public abstract int MaintainCost
    {
        get;
    }
    public abstract int InitialCost
    {
        get;
    }
}
