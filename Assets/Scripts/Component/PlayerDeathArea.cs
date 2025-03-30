using UnityEngine;

public class PlayerDeathArea : TriggerArea
{
    [SerializeField] private Transform returnPoint;
    protected override void Trigger()
    {
        LevelManager levelManager = gameObject.MyLevelManager();
        levelManager.Player.Movement.PlacePlayer(returnPoint.position);
        levelManager.Player.Health.Current = 0;
    }
}
