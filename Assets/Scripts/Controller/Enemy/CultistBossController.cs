using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class CultistBossController : MonoBehaviour
{
    private enum CultistAttackState
    {
        Preparing, Walking, SweepSpray, BurstSpray, Shooting, Spawning
    }    
    private enum CultistMoveState
    {
        Left, Middle, Right, Top, Defeated
    }

    [SerializeField] private float heatRadius;
    private bool activated;

    [Header("Stun")]
    [SerializeField] private EnemyHealth healthComponent;
    [SerializeField] private TMP_Text stunText;
    [SerializeField] private Image portrait;
    [SerializeField] private Sprite normalPortrait;
    [SerializeField] private Sprite stunnedPortrait;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image healthBorder;
    [SerializeField] private Color stunnedColor;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color unshieldedColor;
    [SerializeField] private Sprite unshieldedSprite;
    [SerializeField] private float stunDuration;
    [SerializeField] private ParticleSystem stunParticles;
    [SerializeField] private ParticleSystem respawnParticles;
    [SerializeField] private GameObject disableOnStun;
    [SerializeField] private ParticleSystem flameRing;
    [SerializeField] private DialogueHolder[] stunDialogue;
    [SerializeField] private DialogueHolder firstStunDialogue;
    [SerializeField] private DialogueHolder unshieldDialogue;
    [SerializeField] private DialogueHolder defeatDialogue;
    private bool firstStunned;
    private float stunTimer;

    [Header("Movement")]
    [SerializeField] private CultistAttackState currentAttackState;
    [SerializeField] private CultistAttackState nextAttackState;
    [SerializeField] private CultistMoveState moveState;
    [SerializeField] private Transform flipObject;
    [SerializeField] private Rigidbody2D cultistRigidbody;
    [SerializeField] private Collider2D ringCollider;
    [SerializeField] private Collider2D cultistCollider;
    [SerializeField] private EnemySO cultistInfo;
    [SerializeField] private float acceleration;
    [SerializeField] private Vector2 midRange;
    private Vector2 targetMovement;
    private Vector2 addedVelocity;
    private bool sprayMoving;

    [Header("Attacking")]
    [SerializeField] private float swingRadius;
    [SerializeField] private Vector2 swingPivotPosition;
    [SerializeField] private PlayerShoot.SwingObject[] swingObjects;
    [SerializeField] private Transform nozzle;
    private float aimAngle;
    private Vector2 commitPosition;
    [SerializeField] private float decideDuration;
    private float prepareTimer;

    [Header("Spray")]
    [SerializeField] private CultistFlameSpecial sprayComponent;
    [SerializeField] private float sweepStateDuration;
    [SerializeField] private float sweepArc;
    private float sweepTimer;

    [Header("Burst")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float burstStateDuration;
    [SerializeField] private float burstArc;
    [SerializeField] private AnimationCurve rateOverTime;
    private float burstTimer;
    private float nextShotTimer;

    [Range(1, 100)]
    [SerializeField] private int cacheCapacity;
    private Projectile[] projectileCache;
    private int cacheIndex;

    [Header("Spawning")]
    [SerializeField] private float spawnDuration;
    private float spawnTimer;

    [Header("Building")]
    [SerializeField] private FinalBoss building;

    [Header("Animation")]
    [SerializeField] private Animator cultistAnimator;

    private bool walking;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private Vector2 groundCheckOffset;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private UnityEvent winEvent;

    private void OnEnable()
    {
        building = FindAnyObjectByType<FinalBoss>();
        if (projectileCache == null)
        {
            projectileCache = new Projectile[cacheCapacity];
        }
    }
    private void OnDisable()
    {
        
    }

    public void Activate()
    {
        cultistAnimator.SetBool("IsGrounded", true);
        activated = true;
        transform.parent = null;
        healthComponent.ResetHealth();
        healthComponent.enabled = true;
    }

    void FixedUpdate()
    {
        if (moveState == CultistMoveState.Defeated)
        {
            cultistRigidbody.simulated = false;
            AimSprites(gameObject.MyLevelManager().Player.Position - cultistRigidbody.position);
            return;
        }
        if (!activated)
        {
            cultistRigidbody.simulated = false;
            healthComponent.enabled = false;
            return;
        }
        else if (stunTimer <= 0)
        {
            healthComponent.enabled = true;
            cultistRigidbody.simulated = true;
        }

        if (gameObject.MyLevelManager().levelState != LevelState.Playing)
        {
            cultistRigidbody.linearVelocity = Vector2.zero;
            return;
        }

        if (stunTimer > 0)
        {
            if (!walking)
            {
                cultistRigidbody.linearVelocity = Vector2.zero;
            }
            else
            {
                cultistRigidbody.linearVelocityX = 0;
            }

            stunTimer -= Time.fixedDeltaTime;
            healthComponent.Heal(healthComponent.Max / stunDuration * Time.fixedDeltaTime);

            if (stunTimer <= 0)
            {
                if (moveState != CultistMoveState.Top)
                {
                    healthFill.color = defaultColor;
                }
                else
                {
                    healthComponent.invulnerable = false;
                }

                stunText.text = "";
                portrait.sprite = normalPortrait;
                respawnParticles.Play();
                healthComponent.ResetHealth();
                cultistAnimator.SetTrigger("Respawn");
                flameRing.transform.localScale = Vector2.one;
                disableOnStun.SetActive(true);
                cultistRigidbody.simulated = true;
            }
            return;
        }

        ProximityDamage();

        if (building.Completion >= 1)
        {
            moveState = CultistMoveState.Top;
        }
        else
        {
            Vector2 playerPos = gameObject.MyLevelManager().Player.Position;

            if (playerPos.x < building.transform.position.x + midRange.y && playerPos.x > building.transform.position.x + midRange.x)
            {
                moveState = CultistMoveState.Middle;
            }
            else if (moveState == CultistMoveState.Middle)
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
        }

        if (healthComponent.HealthZero && moveState != CultistMoveState.Defeated)
        {
            Stun();
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
            case CultistMoveState.Top:
                Top();
                break;            
            case CultistMoveState.Defeated:
                DefeatState();
                break;
        }

        Move();
    }

    private void ChooseNextAttack()
    {
        prepareTimer = decideDuration * (1 - building.Completion + 0.2f);
        currentAttackState = CultistAttackState.Preparing;
        int randomState = Random.Range(0, 3);
        switch (randomState)
        {
            case 0:
                nextAttackState = CultistAttackState.SweepSpray;
                break;
            case 1:
                nextAttackState = CultistAttackState.BurstSpray;
                break;
            case 2:
                nextAttackState = CultistAttackState.Spawning;
                break;
        }
    }

    private void PreparingAttackState()
    {
        if (Vector2.Distance(gameObject.MyLevelManager().Player.Position, cultistRigidbody.position) > cultistInfo.attackRange)
        {
            return;
        }

        if (prepareTimer > 0)
        {
            prepareTimer -= Time.fixedDeltaTime;
            return;
        }

        switch (nextAttackState)
        {
            case CultistAttackState.SweepSpray:
                sweepTimer = sweepStateDuration;
                commitPosition = gameObject.MyLevelManager().Player.Position;
                break;            
            case CultistAttackState.BurstSpray:
                burstTimer = burstStateDuration;
                if (moveState == CultistMoveState.Middle)
                {
                    commitPosition = cultistRigidbody.position + Vector2.up;
                }
                else
                {
                    commitPosition = gameObject.MyLevelManager().Player.Position;
                }
                nextShotTimer = 0;
                break;
            case CultistAttackState.Spawning:
                if (moveState == CultistMoveState.Middle && building.CurrentFloor.connector.Spawned)
                {
                    ChooseNextAttack();
                    return;
                }
                spawnTimer = spawnDuration;
                break;
        }

        currentAttackState = nextAttackState;
    }
    private void RightSide()
    {
        if (!building.CurrentFloor.rightStaircase.GlassBroken)
        {
            Vector2 targetPos = new Vector2(building.transform.position.x + midRange.x + 18, building.CurrentFloorLevel);
            AimSprites(gameObject.MyLevelManager().Player.Position);
            MoveToPosition(targetPos);
            building.CurrentFloor.rightStaircase.ActivateArm();
            return;
        }

        switch (currentAttackState)
        {
            case CultistAttackState.Preparing:
                PreparingAttackState();
                FollowPlayerVertical(building.transform.position.x + midRange.y + 18);
                break;
            case CultistAttackState.SweepSpray:
                if (!sprayMoving)
                {
                    SweepSprayState();
                }
                else
                {
                    FollowPlayerVertical(building.transform.position.x + midRange.y + 18);
                }
                break;
            case CultistAttackState.BurstSpray:
                if (!sprayMoving)
                {
                    BurstSprayState();
                }
                else
                {
                    FollowPlayerVertical(building.transform.position.x + midRange.y + 18);
                }
                break;
            case CultistAttackState.Spawning:
                ChooseNextAttack();
                break;
        }
    }
    private void Middle()
    {
        switch (currentAttackState)
        {
            case CultistAttackState.Preparing:
                PreparingAttackState();
                FollowPlayerHorizontal(building.CurrentFloorLevel + 2);
                break;
            case CultistAttackState.SweepSpray:
                if (!sprayMoving)
                {
                    SweepSprayState();
                }
                else
                {
                    FollowPlayerHorizontal(building.CurrentFloorLevel + 2);
                }
                break;
            case CultistAttackState.BurstSpray:
                if (!sprayMoving)
                {
                    BurstSprayState();
                }
                else
                {
                    FollowPlayerHorizontal(building.CurrentFloorLevel + 2);
                }
                break;            
            case CultistAttackState.Spawning:
                MoveToPosition(new Vector2(building.transform.position.x + (midRange.x + midRange.y) / 2, building.CurrentFloorLevel + 1.5f));
                SpawnState();
                break;
        }
    }
    private void LeftSide()
    {
        if (!building.CurrentFloor.leftStaircase.GlassBroken)
        {
            Vector2 targetPos = new Vector2(building.transform.position.x + midRange.x - 18, building.CurrentFloorLevel);
            AimSprites(gameObject.MyLevelManager().Player.Position);
            MoveToPosition(targetPos);
            building.CurrentFloor.leftStaircase.ActivateArm();
            return;
        }

        switch (currentAttackState)
        {
            case CultistAttackState.Preparing:
                PreparingAttackState();
                FollowPlayerVertical(building.transform.position.x + midRange.x - 18);
                break;
            case CultistAttackState.SweepSpray:
                if (!sprayMoving)
                {
                    SweepSprayState();
                }
                else
                {
                    FollowPlayerVertical(building.transform.position.x + midRange.x - 18);
                }
                break;
            case CultistAttackState.BurstSpray:
                if (!sprayMoving)
                {
                    BurstSprayState();
                }
                else
                {
                    FollowPlayerVertical(building.transform.position.x + midRange.x - 18);
                }
                break;
            case CultistAttackState.Spawning:
                ChooseNextAttack();
                break;
        }
    }

    private void Top()
    {
        if (cultistRigidbody.position.x > building.transform.position.x + midRange.y || cultistRigidbody.position.x < building.transform.position.x + midRange.x)
        {
            MoveToPosition(new Vector2(building.transform.position.x + (midRange.x + midRange.y) / 2, building.CurrentFloorLevel + 8.5f));
            return;
        }

        switch (currentAttackState)
        {
            case CultistAttackState.Preparing:
                PreparingAttackState();
                FollowPlayerHorizontal(building.CurrentFloorLevel + 8.5f);
                break;
            case CultistAttackState.SweepSpray:
                if (!sprayMoving)
                {
                    cultistAnimator.SetBool("IsWalking", false);
                    SweepSprayState();
                }
                else
                {
                    FollowPlayerHorizontal(building.CurrentFloorLevel + 8.5f);
                }
                break;
            case CultistAttackState.BurstSpray:
                if (!sprayMoving)
                {
                    cultistAnimator.SetBool("IsWalking", false);
                    BurstSprayState();
                }
                else
                {
                    FollowPlayerHorizontal(building.CurrentFloorLevel + 8.5f);
                }
                break;
            case CultistAttackState.Spawning:
                FollowPlayerHorizontal(building.CurrentFloorLevel + 8.5f);
                SpawnState();
                break;
        }
    }

    private void DefeatState()
    {

    }

    public void Defeat()
    {
        HelicopterDescend.Descend();
    }


    private void SweepSprayState()
    {
        targetMovement = Vector2.zero;
        if (sweepTimer <= 0)
        {
            ChooseNextAttack();
            return;
        }
        sweepTimer -= Time.fixedDeltaTime;

        float addAngle = Mathf.Lerp(-sweepArc * 0.5f, sweepArc * 0.5f, sweepTimer / sweepStateDuration);

        if (commitPosition.x > cultistRigidbody.position.x)
        {
            addAngle *= -1;
        }

        AimSprites(commitPosition);
        sprayComponent.AimAttack(nozzle.position, aimAngle + addAngle);

        Vector2 pushDirection = Quaternion.Euler(0, 0, aimAngle + addAngle) * Vector2.left;
        addedVelocity += pushDirection * 0.3f;
    }
    private void BurstSprayState()
    {
        if (burstTimer <= 0)
        {
            ChooseNextAttack();
            return;
        }
        targetMovement = Vector2.zero;
        burstTimer -= Time.fixedDeltaTime;

        float addAngle = Mathf.Lerp(-burstArc * 0.5f, burstArc * 0.5f, burstTimer / burstStateDuration);

        if (commitPosition.x < cultistRigidbody.position.x)
        {
            addAngle *= -1;
        }

        Vector2 commitDirection = commitPosition - cultistRigidbody.position;
        float shootAngle = Mathf.Rad2Deg * Mathf.Atan2(commitDirection.y, commitDirection.x);
        Vector2 shootDirection = Quaternion.Euler(0, 0, shootAngle + addAngle) * Vector2.right;

        AimSprites(cultistRigidbody.position + shootDirection);

        if (nextShotTimer <= 0)
        {
            ThrowProjectile(shootDirection);
            nextShotTimer = rateOverTime.Evaluate(burstTimer / burstStateDuration);
        }
        else
        {
            nextShotTimer -= Time.fixedDeltaTime;
        }
    }

    private void SpawnState()
    {
        if (!building.CurrentFloor.connector.Spawned)
        {
            building.CurrentFloor.connector.InitiateSpawn();
        }
        else
        {
            spawnTimer = 0;
            ChooseNextAttack();
        }
        if (spawnTimer > 0)
        {
            spawnTimer -= Time.fixedDeltaTime;

            if (spawnTimer <= 0)
            {
                if (moveState == CultistMoveState.Top)
                {
                    building.CurrentFloor.connector.Spawned = false;
                }
                ChooseNextAttack();
            }
        }
    }

    private void FollowPlayerVertical(float xPosition)
    {
        Vector2 targetPos = new Vector2(xPosition, gameObject.MyLevelManager().Player.Position.y + 0.5f);
        AimSprites(gameObject.MyLevelManager().Player.Position);

        MoveToPosition(targetPos);
    }

    private void FollowPlayerHorizontal(float yPosition)
    {
        Vector2 targetPos = new Vector2(gameObject.MyLevelManager().Player.Position.x, yPosition);
        Vector2 targetDirection = targetPos - cultistRigidbody.position;
        AimSprites(gameObject.MyLevelManager().Player.Position);

        float targetDistance = targetDirection.magnitude - cultistInfo.standRange;

        if (targetDistance > 0)
        {
            MoveToPosition(targetPos);
        }
        else
        {
            targetMovement *= 0.92f;
        }
    }

    protected void ThrowProjectile(Vector2 direction)
    {
        Projectile projectile = null;
        if (projectileCache[cacheIndex] == null)
        {
            projectile = Instantiate(projectilePrefab, nozzle.position, Quaternion.identity, null);
            projectileCache[cacheIndex] = projectile;
        }
        else
        {
            projectile = projectileCache[cacheIndex];
            projectile.transform.position = nozzle.position;
        }

        cacheIndex = (cacheIndex + 1) % cacheCapacity;

        projectile.ResetProjectile();
        projectile.Propel(direction.normalized * projectileSpeed);
    }

    private void MoveToPosition(Vector2 targetPos)
    {
        sprayMoving = false;
        Vector2 targetDirection = targetPos - cultistRigidbody.position;
        if (Mathf.Abs(cultistRigidbody.position.x - targetPos.x) > 1)
        {
            targetMovement.x += acceleration * targetDirection.normalized.x;
            targetMovement.x = Mathf.Clamp(targetMovement.x, -cultistInfo.speed, cultistInfo.speed);
        }
        else
        {
            if (!walking)
            {
                targetMovement.x *= 0.92f;
            }
            else
            {
                targetMovement.x *= 0.8f;
            }
        }

        if (Mathf.Abs(cultistRigidbody.position.y - targetPos.y) > 1)
        {
            targetMovement.y += acceleration * targetDirection.normalized.y;
            targetMovement.y = Mathf.Clamp(targetMovement.y, -cultistInfo.speed * 0.4f, cultistInfo.speed * 0.4f);
        }
        else
        {
            targetMovement.y *= 0.92f;
        }

        if (targetDirection.magnitude > cultistInfo.aggroRange)
        {
            AimSprites((Vector2)transform.position - targetDirection + Vector2.down * 1f);
            sprayComponent.AimAttack(nozzle.position, aimAngle);

            Vector2 pushDirection = Quaternion.Euler(0, 0, aimAngle) * Vector2.left;

            if (!walking)
            {
                addedVelocity += pushDirection * 0.3f;
            }
            else
            {
                pushDirection.y *= 20;
                addedVelocity += pushDirection * 0.3f;
            }
            sprayMoving = true;
        }
    }

    private void Move()
    {
        if (walking)
        {
            cultistAnimator.SetFloat("MoveSpeed", Mathf.Clamp(targetMovement.x, -1, 1));
            cultistAnimator.SetBool("IsWalking", true);
            if (Physics2D.OverlapCircle(groundCheckOffset, groundCheckRadius, groundLayers))
            {
                addedVelocity.y = 0;
            }
            else
            {
                addedVelocity.y -= 18 * Time.fixedDeltaTime;
            }
            targetMovement.y = 0;

            if (targetMovement.x == 0)
            {
                cultistAnimator.SetBool("IsWalking", false);
            }
        }
        cultistRigidbody.linearVelocity = targetMovement + addedVelocity;

        if (addedVelocity.magnitude > 0.2f)
        {
            addedVelocity *= 0.98f;
        }
        else
        {
            addedVelocity = Vector2.zero;
        }
    }

    private void ProximityDamage()
    {
        Vector2 playerPosition = gameObject.MyLevelManager().Player.Position;
        float playerDistance = Vector2.Distance(playerPosition, transform.position);

        // Checking if the fire should damage the player
        if (playerDistance < heatRadius)
        {
            gameObject.MyLevelManager().Player.Health.FireDamage(2f * Mathf.Pow(0.5f, playerDistance));
        }
    }

    public void SetCamera(bool cultistView)
    {
        if (cultistView)
        {
            building.SetCameraPosition(transform.position + Vector3.down);
        }
        else
        {
            building.ResetCamera();
        }
    }

    private void Stun()
    {
        if (!walking)
        {
            stunTimer = stunDuration;
            cultistAnimator.SetTrigger("IsDead");
        }
        if (moveState == CultistMoveState.Top && cultistRigidbody.position.x < building.transform.position.x + midRange.y && cultistRigidbody.position.x > building.transform.position.x + midRange.x)
        {
            if (!walking)
            {
                unshieldDialogue.PlayDialogue();
                healthComponent.invulnerable = true;
                healthBorder.sprite = unshieldedSprite;
                healthFill.color = unshieldedColor;
                flameRing.gameObject.SetActive(false);
                cultistRigidbody.gravityScale = 1;
                ringCollider.enabled = false;
                cultistCollider.enabled = true;
                walking = true;
            }
            else
            {
                healthComponent.enabled = false;
                moveState = CultistMoveState.Defeated;
                winEvent.Invoke();
                defeatDialogue.PlayDialogue();
                stunParticles.Play();
                disableOnStun.SetActive(false);
                return;
            }
        }
        else
        {
            healthFill.color = stunnedColor;
            cultistRigidbody.simulated = false;
            if (!firstStunned)
            {
                firstStunned = true;
                firstStunDialogue.PlayDialogue();
            }
            else if (stunDialogue.Length > 0)
            {
                int random = Random.Range(0, stunDialogue.Length);
                stunDialogue[random].PlayDialogue();
            }
        }

        stunText.text = "STUNNED!";
        portrait.sprite = stunnedPortrait;
        stunParticles.Play();
        flameRing.transform.localScale = Vector2.one * 0.4f;
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
