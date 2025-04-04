using UnityEngine;

public class CultistBossController : MonoBehaviour
{
    private enum CultistBossState
    {
        Flying, Walking, SweepSpray, BurstSpray, Shooting
    }
    [SerializeField] private CultistBossState currentState;
    [SerializeField] private float swingRadius;
    [SerializeField] private Vector2 swingPivotPosition;
    [SerializeField] private PlayerShoot.SwingObject[] swingObjects;
    [SerializeField] private Transform flipObject;
    [SerializeField] private Transform nozzle;
    private float aimAngle;
    private Vector2 commitPosition;

    [SerializeField] private float decideDuration;
    private float decideTimer;

    [Header("Spray")]
    [SerializeField] private float sweepStateDuration;
    [SerializeField] private float sweepArc;
    private float sweepTimer;
    [SerializeField] private float burstStateDuration;
    [SerializeField] private float burstDuration;
    [SerializeField] private int bursts;
    private float burstTimer;
    [SerializeField] private CultistFlameSpecial sprayComponent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameObject.MyLevelManager().levelState != LevelState.Playing)
        {
            return;
        }
        switch (currentState)
        {
            case CultistBossState.Flying:
                FlyingState();
                break;
            case CultistBossState.SweepSpray:
                SweepSprayState();
                break;
            case CultistBossState.BurstSpray:
                SweepSprayState();
                break;
        }
    }

    private void ReturnToFlying()
    {
        decideTimer = decideDuration;
        currentState = CultistBossState.Flying;
    }
    private void FlyingState()
    {
        if (decideTimer > 0)
        {
            decideTimer -= Time.fixedDeltaTime;
            return;
        }

        int randomState = Random.Range(0, 1);
        switch (randomState)
        {
            case 0:
                sweepTimer = sweepStateDuration;
                commitPosition = gameObject.MyLevelManager().Player.Position;
                currentState = CultistBossState.SweepSpray;
                break;
        }
    }
    private void SweepSprayState()
    {
        if (sweepTimer <= 0)
        {
            ReturnToFlying();
            return;
        }
        sweepTimer -= Time.fixedDeltaTime;

        float addAngle = Mathf.Lerp(-sweepArc * 0.5f, sweepArc * 0.5f, sweepTimer / sweepStateDuration);
        AimSprites(commitPosition);
        sprayComponent.AimAttack(nozzle.position, aimAngle + addAngle);
    }
    private void BurstSprayState()
    {
        AimSprites(gameObject.MyLevelManager().Player.Position);
        sprayComponent.AimAttack(nozzle.position, aimAngle);
    }

    public void AimSprites(Vector2 targetPosition)
    {
        //For every object that needs to point towards the mouse
        for (int i = 0; i < swingObjects.Length; i++)
        {
            Vector2 spriteDirection = targetPosition - (Vector2)swingObjects[i].objectTransform.position;
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
}
