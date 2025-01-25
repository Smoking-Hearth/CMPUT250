using UnityEngine;

public interface ISpecialAttack
{
    public void Activate(Vector2 startPosition, bool active);
    public void ResetAttack(float angle);
    public void AimAttack(Vector2 startPosition, float angle);
}
