using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        stWaiting,
        stDefeated,
        stTargeting, // Either Aiming at player OR Moving towards/away from player
        stFrontSwing,
        stDuringAttack,
        stBackSwing
    };

    [SerializeField] protected EnemySO enemyInfo;

    private ParticleSystem spawnParticles;

    [Tooltip("The part of the enemy that can actually interact with the world")]
    [SerializeField] protected Transform body;
    [SerializeField] protected EnemyHealth healthComponent;
    [SerializeField] protected LayerMask waterLayer;
    protected Vector2 targetPosition;
    protected float distance;
    public bool cannotDamage = false;
    public bool canMove = true;

    private Animator enemyAnimator;

    protected Transform attackVisual; //PLACEHOLDER
    protected float commitAttackTimer;
    protected float frontSwingTimer;
    protected float backSwingTimer;
    protected float defeatTimer;

    public EnemyState currentState = EnemyState.stWaiting;

    [Header("Sound")]
    [SerializeField] protected FireSounds sounds;

    public bool Active
    {
        get
        {
            return currentState != EnemyState.stDefeated;
        }
    }

    protected virtual void OnEnable()
    {
        healthComponent.EnemyInfo = enemyInfo;
        healthComponent.Current = healthComponent.Max;
        commitAttackTimer = enemyInfo.commitAttackSeconds;
        frontSwingTimer = enemyInfo.frontSwingSeconds;
        backSwingTimer = enemyInfo.backSwingSeconds;
        enemyAnimator = gameObject.GetComponent<Animator>();
        if (attackVisual == null)
        {
            if (enemyInfo.attackPrefab != null)
            {
                attackVisual = Instantiate(enemyInfo.attackPrefab, Vector2.zero, Quaternion.identity, null);
                attackVisual.gameObject.SetActive(false);
            }
        }
        else
        {
            attackVisual.gameObject.SetActive(false);
        }

        if (spawnParticles != null)
        {
            Destroy(spawnParticles);
        }
        spawnParticles = Instantiate(enemyInfo.spawnParticles, Vector2.zero, Quaternion.identity, transform);

        healthComponent.onHurt += Hurt;
        currentState = EnemyState.stWaiting;
    }

    protected virtual void OnDisable()
    {
        healthComponent.onHurt -= Hurt;
    }

    protected virtual void FixedUpdate()
    {
        if (!gameObject.ShouldUpdate()) return;

        LevelManager lm = gameObject.MyLevelManager();
        distance = Vector2.Distance(transform.position, lm.Player.Position);

        if (lm.levelState != LevelState.Playing)
        {
            currentState = EnemyState.stWaiting;
            if (attackVisual != null)
            {
                attackVisual.gameObject.SetActive(false);
            }
        }

        switch (currentState)
        {
            case EnemyState.stWaiting:
                Idle();
                break;
            case EnemyState.stDefeated:
                if (defeatTimer > 0)
                {
                    defeatTimer -= Time.fixedDeltaTime;
                    break;
                }
                gameObject.SetActive(false);
                break;
            case EnemyState.stTargeting:
                Target();
                break;
            case EnemyState.stFrontSwing:
                FrontSwing();
                break;
            case EnemyState.stDuringAttack:
                Attack();
                break;
            case EnemyState.stBackSwing:
                BackSwing();
                break;
        }

    }
    public virtual void Hurt()
    {
        if (healthComponent.HealthZero)
        {
            currentState = EnemyState.stDefeated;
            defeatTimer = enemyInfo.defeatDurationSeconds;
            sounds.ExtinguishSound();
        }
        else
        {
            sounds.HitSound();
        }
    }

    protected virtual void OnCollisionEnter2D(Collision2D col)
    {
        if (waterLayer != col.gameObject.layer) return;

        var damage = col.gameObject.GetComponent<Damage>();
        if (damage == null) return;

        // Time to get hurt.
        var health = GetComponent<Health>();

        if (health == null)
        {
            Debug.LogWarning("There exists an invulnerable enemy");
        }
        else if (cannotDamage) return;
        else
        {
            health.Current -= damage.damage;
        }
    }

    protected virtual void Idle()
    {
        // Debug.Log("We are idle");
        //
        // PLay idle animation? Sounds?
        //
        if (distance < enemyInfo.aggroRange)
        {
            currentState = EnemyState.stTargeting;
        }

    }

    protected virtual void Target()
    {
        targetPosition = gameObject.MyLevelManager().Player.Position;

        // Face Target, aim at them?
        // Walk towards them, assuming they can do that?
        // canMove = true;
        if (distance < enemyInfo.aggroRange && canMove)
        {   

            enemyAnimator.SetBool("IsMoving", true); // MIGHT HAVE TO EDIT !!!!!!!!!!!!!!!!!
            MoveToTarget();
            if (distance <= enemyInfo.attackRange)
            {
                commitAttackTimer -= Time.fixedDeltaTime;

                if (commitAttackTimer <= 0)
                {
                    currentState = EnemyState.stFrontSwing;
                }
            }
            // If Target steps out of range? Reset Timer slightly, go back to targetting
            else
            {
                commitAttackTimer = enemyInfo.commitAttackSeconds / 2f;
                currentState = EnemyState.stTargeting;
            }
        }
        else
        {
            enemyAnimator.SetBool("IsMoving", false); // PROBABLY HAVE TO EDIT !!!!!!!!!!!!!!!!!
            currentState = EnemyState.stWaiting;
        }
    }

    protected virtual void MoveToTarget()
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;
        
        // NOTE: We need to retain z otherwise the enemy will be culled by Camera
        Vector3 position = (Vector2)transform.position + direction.normalized * Time.fixedDeltaTime * enemyInfo.speed;
        position.z = transform.position.z;
        transform.position = position;

        if (direction.x < 0)
        {
            body.localScale = new Vector2(-1, 1);
        }
        else
        {
            body.localScale = Vector2.one;
        }
    }

    protected virtual void FrontSwing()
    {
        if (frontSwingTimer > 0)
        {
            frontSwingTimer -= Time.fixedDeltaTime;
            return;
        }
        frontSwingTimer = enemyInfo.frontSwingSeconds;
        enemyAnimator.SetTrigger(1); // PROBABLY HAVE TO EDIT !!!!!!!!
        currentState = EnemyState.stDuringAttack;
    }

    protected virtual void BackSwing()
    {
        if (backSwingTimer > 0)
        {
            backSwingTimer -= Time.fixedDeltaTime;
            return;
        }

        if (attackVisual != null)
        {
            attackVisual.gameObject.SetActive(false);
        }
        backSwingTimer = enemyInfo.backSwingSeconds;
        currentState = EnemyState.stTargeting;
    }

    protected virtual void Attack()
    {
        if (GameManager.onEnemyAttack != null)
        {
            Vector2 attackCenter = (Vector2)transform.position + (targetPosition - (Vector2)transform.position).normalized * enemyInfo.attackRange * 0.5f;
            if (attackVisual != null)
            {
                attackVisual.position = attackCenter;
                attackVisual.gameObject.SetActive(true);
            }

            GameManager.onEnemyAttack(attackCenter, transform.position, enemyInfo.attackInfo);
        }
        commitAttackTimer = enemyInfo.commitAttackSeconds;
        currentState = EnemyState.stBackSwing;
    }
}
