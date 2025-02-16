using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour, IExtinguishable
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

    [Tooltip("The part of the enemy that can actually interact with the world")]
    [SerializeField] protected Transform body;
    [SerializeField] protected EnemyHealth healthComponent;
    [SerializeField] protected CombustibleKind fireKind;
    [SerializeField] protected LayerMask waterLayer;
    [SerializeField] protected EnemyAttackInfo attackInfo;
    protected Vector2 targetPosition;
    public float speed;
    protected float distance;
    public bool cannotDamage = false;
    public bool canMove = true;

    [SerializeField] protected Transform attackVisual; //PLACEHOLDER
    [SerializeField] protected float aggroRange = 5f;
    [SerializeField] protected float attackRange = 2f;

    [SerializeField] protected float commitAttackSeconds = 1.5f;
    protected float commitAttackTimer;

    [SerializeField] protected float frontSwingSeconds;
    protected float frontSwingTimer;

    [SerializeField] protected float backSwingSeconds;
    protected float backSwingTimer;

    [SerializeField] protected float defeatDurationSeconds;
    protected float defeatTimer;

    //public ScriptableObject FlameDash;

    // Centre of Attacl
    // Radious of Attack
    // Radius of attack hitbox

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
        healthComponent.Current = healthComponent.Max;
        commitAttackTimer = commitAttackSeconds;
        frontSwingTimer = frontSwingSeconds;
        backSwingTimer = backSwingSeconds;
        if (attackVisual != null)
        {
            attackVisual.gameObject.SetActive(false);
        }
    }


    protected virtual void FixedUpdate()
    {
        if (!gameObject.ShouldUpdate()) return;

        distance = Vector2.Distance(transform.position, LevelManager.Active.Player.Position);

        if (LevelManager.Active.levelState != LevelState.Playing)
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
    public virtual void Extinguish(CombustibleKind extinguishClass, float quantity_L)
    {
        if (healthComponent.HealthZero)
        {
            return;
        }
        if ((extinguishClass & fireKind) == 0)
        {
            if (fireKind == CombustibleKind.C_ELECTRICAL)
            {
                Vector2 direction = (Vector2)transform.position - LevelManager.Active.Player.Position;
                GameManager.onEnemyAttack(LevelManager.Active.Player.Position + direction.normalized * 0.5f, transform.position, GameManager.FireSettings.electricBackfire);
            }
            return;
        }

        healthComponent.Hurt(extinguishClass, quantity_L);

        if (healthComponent.HealthZero)
        {
            sounds.ExtinguishSound();
            CompleteExtinguish();
        }
        else
        {
            sounds.HitSound();
        }
    }

    public virtual void CompleteExtinguish()
    {
        currentState = EnemyState.stDefeated;
        defeatTimer = defeatDurationSeconds;
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
        if (distance < aggroRange)
        {
            currentState = EnemyState.stTargeting;
        }

    }

    protected virtual void Target()
    {
        targetPosition = LevelManager.Active.Player.Position;

        // Face Target, aim at them?
        // Walk towards them, assuming they can do that?
        // canMove = true;
        if (distance < aggroRange && canMove)
        {
            MoveToTarget();
            if (distance <= attackRange)
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
                commitAttackTimer = commitAttackSeconds / 2f;
                currentState = EnemyState.stTargeting;
            }
        }
        else
        {
            currentState = EnemyState.stWaiting;
        }
    }

    protected virtual void MoveToTarget()
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;
        transform.position = (Vector2)transform.position + direction.normalized * Time.fixedDeltaTime * speed;
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
        frontSwingTimer = frontSwingSeconds;
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
        backSwingTimer = backSwingSeconds;
        currentState = EnemyState.stTargeting;
    }

    protected virtual void Attack()
    {
        if (GameManager.onEnemyAttack != null)
        {
            Vector2 attackCenter = (Vector2)transform.position + (targetPosition - (Vector2)transform.position).normalized * attackRange;
            if (attackVisual != null)
            {
                attackVisual.position = attackCenter;
                attackVisual.gameObject.SetActive(true);
            }

            GameManager.onEnemyAttack(attackCenter, transform.position, attackInfo);
        }
        commitAttackTimer = commitAttackSeconds;
        currentState = EnemyState.stBackSwing;
    }
}
