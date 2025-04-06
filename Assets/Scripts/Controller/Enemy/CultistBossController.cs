using UnityEngine;

public class CultistBossController : MonoBehaviour
{
    private enum CultistAttackState
    {
        Choosing, Walking, SweepSpray, BurstSpray, Shooting
    }    
    private enum CultistMoveState
    {
        Left, Middle, Right
    }

    [Header("Stun")]
    [SerializeField] private EnemyHealth healthComponent;
    [SerializeField] private float stunDuration;
    [SerializeField] private ParticleSystem stunParticles;
    [SerializeField] private GameObject disableOnStun;
    private float stunTimer;

    [Header("Movement")]
    [SerializeField] private CultistAttackState currentState;
    [SerializeField] private CultistMoveState moveState;
    [SerializeField] private Transform flipObject;
    [SerializeField] private Rigidbody2D cultistRigidbody;
    [SerializeField] private EnemySO cultistInfo;
    [SerializeField] private float acceleration;
    [SerializeField] private Vector2 midRange;

    [Header("Attacking")]
    [SerializeField] private float swingRadius;
    [SerializeField] private Vector2 swingPivotPosition;
    [SerializeField] private PlayerShoot.SwingObject[] swingObjects;
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

    [Header("Building")]
    [SerializeField] private FinalBoss building;

    [Header("Animation")]
    [SerializeField] private Animator cultistAnimator;

    private void OnEnable()
    {
    }
    private void OnDisable()
    {
        
    }

    void FixedUpdate()
    {
        cultistAnimator.SetBool("IsGrounded", true);

        if (stunTimer > 0)
        {
            cultistRigidbody.linearVelocity = Vector2.zero;
            stunTimer -= Time.fixedDeltaTime;

            if (stunTimer <= 0)
            {
                healthComponent.ResetHealth();
                cultistAnimator.SetTrigger("Respawn");
                disableOnStun.SetActive(true);
            }
            return;
        }
        Vector2 playerPos = gameObject.MyLevelManager().Player.Position;
        if (playerPos.x < building.transform.position.x + midRange.y && playerPos.x > building.transform.position.x + midRange.x)
        {
            moveState = CultistMoveState.Middle;
        }
        else
        {
            if (playerPos.x <= building.transform.position.x + midRange.x)
            {
                moveState = CultistMoveState.Left;
            }
            else if (playerPos.x >= building.transform.position.x + midRange.y)
            {
                moveState = CultistMoveState.Right;
            }
        }

        if (healthComponent.HealthZero)
        {
            Stun();
        }

        if (gameObject.MyLevelManager().levelState != LevelState.Playing)
        {
            return;
        }
        switch (moveState)
        {
            case CultistMoveState.Right:
                RightSide();
                break;
            case CultistMoveState.Middle:
                Middle();
                break;
            case CultistMoveState.Left:
                LeftSide();
                break;
        }
    }

    private void ReturnToChoosing()
    {
        decideTimer = decideDuration;
        currentState = CultistAttackState.Choosing;
    }

    private void ChoosingState()
    {
        if (Vector2.Distance(gameObject.MyLevelManager().Player.Position, cultistRigidbody.position) > cultistInfo.attackRange)
        {
            return;
        }

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
                currentState = CultistAttackState.SweepSpray;
                break;
        }
    }
    private void RightSide()
    {
        switch (currentState)
        {
            case CultistAttackState.Choosing:
                ChoosingState();
                FollowPlayerVertical(building.transform.position.x + midRange.y + 15);
                break;
            case CultistAttackState.SweepSpray:
                SweepSprayState();
                break;
            case CultistAttackState.BurstSpray:
                SweepSprayState();
                break;
        }
    }
    private void Middle()
    {
        switch (currentState)
        {
            case CultistAttackState.Choosing:
                ChoosingState();
                FollowPlayerHorizontal(gameObject.MyLevelManager().Player.Position.y + 0.5f);
                break;
            case CultistAttackState.SweepSpray:
                SweepSprayState();
                break;
            case CultistAttackState.BurstSpray:
                SweepSprayState();
                break;
        }
    }
    private void LeftSide()
    {
        switch (currentState)
        {
            case CultistAttackState.Choosing:
                ChoosingState();
                FollowPlayerVertical(building.transform.position.x + midRange.x - 15);
                break;
            case CultistAttackState.SweepSpray:
                SweepSprayState();
                break;
            case CultistAttackState.BurstSpray:
                SweepSprayState();
                break;
        }
    }
    private void SweepSprayState()
    {
        cultistRigidbody.linearVelocity = Vector2.zero;
        if (sweepTimer <= 0)
        {
            ReturnToChoosing();
            return;
        }
        sweepTimer -= Time.fixedDeltaTime;

        float addAngle = Mathf.Lerp(-sweepArc * 0.5f, sweepArc * 0.5f, sweepTimer / sweepStateDuration);

        if (moveState == CultistMoveState.Left)
        {
            addAngle *= -1;
        }

        AimSprites(commitPosition);
        sprayComponent.AimAttack(nozzle.position, aimAngle + addAngle);
    }
    private void BurstSprayState()
    {
        AimSprites(gameObject.MyLevelManager().Player.Position);
        sprayComponent.AimAttack(nozzle.position, aimAngle);
    }

    private void FollowPlayerVertical(float xPosition)
    {
        Vector2 targetPos = new Vector2(xPosition, gameObject.MyLevelManager().Player.Position.y);
        Vector2 targetDirection = targetPos - cultistRigidbody.position;
        AimSprites(gameObject.MyLevelManager().Player.Position);

        if (targetDirection.magnitude > 1f)
        {
            cultistRigidbody.linearVelocity += acceleration * targetDirection.normalized;
            cultistRigidbody.linearVelocityX = Mathf.Clamp(cultistRigidbody.linearVelocityX, -cultistInfo.speed, cultistInfo.speed);
            cultistRigidbody.linearVelocityY = Mathf.Clamp(cultistRigidbody.linearVelocityY, -cultistInfo.speed, cultistInfo.speed);
        }
        else
        {
            cultistRigidbody.linearVelocity *= 0.92f;
        }
    }

    private void FollowPlayerHorizontal(float yPosition)
    {
        Vector2 targetPos = new Vector2(gameObject.MyLevelManager().Player.Position.x, yPosition);
        Vector2 targetDirection = targetPos - cultistRigidbody.position;
        AimSprites(gameObject.MyLevelManager().Player.Position);

        if (targetDirection.magnitude > cultistInfo.standRange)
        {
            cultistRigidbody.linearVelocity += acceleration * targetDirection.normalized;
            cultistRigidbody.linearVelocityX = Mathf.Clamp(cultistRigidbody.linearVelocityX, -cultistInfo.speed, cultistInfo.speed);
            cultistRigidbody.linearVelocityY = Mathf.Clamp(cultistRigidbody.linearVelocityY, -cultistInfo.speed, cultistInfo.speed);

        }
        else
        {
            cultistRigidbody.linearVelocity *= 0.92f;
        }
    }

    private void Stun()
    {
        stunParticles.Play();
        stunTimer = stunDuration;
        cultistAnimator.SetTrigger("IsDead");
        disableOnStun.SetActive(false);
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
