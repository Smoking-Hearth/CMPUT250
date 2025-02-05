using System.Collections;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Mono.Cecil;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour 
{

    static int waterLayer;

    // public CircleCollider2D hitbox;
    public float damage = 25f;
    
    private Transform target;
    public float speed;
    public float distance;
    public bool cannotDamage = false;
    public bool canMove = true;
    public float maxRange = 5f;

    private float attackTimer;
    public float attackCooldown = 1.5f;

    public float touchDist = 1.05f;

    

    //public ScriptableObject FlameDash;
    public enum EnemyState{
        stWaiting,
        stTargeting, // Either Aiming at player OR Moving towards/away from player
        stBeforeAttack,
        stDuringAttack
    };

    // Centre of Attacl
    // Radious of Attack
    // Radius of attack hitbox

    public EnemyState currentState = EnemyState.stWaiting;

    void Awake() 
    {
        attackTimer = attackCooldown;
        waterLayer = LayerMask.NameToLayer("Water");
        GetComponent<Health>().OnDepleted += () => {
            Destroy(gameObject);
        };

        // hitbox = GetComponent<CircleCollider2D>();
        
        // Target will always be player
        target = GameObject.FindGameObjectWithTag("Target").GetComponent<Transform>();

    }   
    
    void OnCollisionEnter2D(Collision2D col)
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
            health.current -= damage.damage;
        }
        Destroy(col.gameObject);
    }

    IEnumerator AttackCooldown(){

        yield return new WaitForSeconds(attackCooldown);
        attackTimer = attackCooldown;
        currentState = EnemyState.stTargeting;
    }

    // void OnTriggerEnter2D(Collider2D hitbox){

    //     if (hitbox.CompareTag("Player")){

    //         Debug.Log("We are burning them alive!");
    //         //damage += playerHealth
    //         // Knock player back?

    //     }
    //     else return;

    // }

    void Update(){
    
        distance = Vector2.Distance(transform.position, target.position);

        switch (currentState){
            case EnemyState.stWaiting:
                Idle();
                break;
            case EnemyState.stTargeting:
                Target();
                break;
            case EnemyState.stBeforeAttack:
                Attack(gameObject.tag);
                break;
            case EnemyState.stDuringAttack: break;
        }
    }

    void Idle(){
        // Debug.Log("We are idle");
        //
        // PLay idle animation? Sounds?
        //
        if (distance < maxRange){
            currentState = EnemyState.stTargeting;
        }

    }

    void Target(){

        if(currentState == EnemyState.stBeforeAttack || currentState == EnemyState.stDuringAttack) return;

        // Face Target, aim at them?
        // Walk towards them, assuming they can do that?
        // canMove = true;


        if (distance < maxRange && canMove){
            
            
            transform.position = Vector2.MoveTowards(transform.position, target.position, Time.deltaTime * speed);

            attackTimer -= Time.deltaTime;

            // Testing for when player within range
            // if (distance < touchDist) {
                
            //     Debug.Log("We are burning them alive!");
            //     // damage += playerHealth

            // }
            if (attackTimer <= 0){

                currentState = EnemyState.stBeforeAttack;

            }
        }
        // If Target steps out of range? Reset Timer, go back to idle (Allegedly)
        else {attackTimer = attackCooldown; currentState = EnemyState.stWaiting;}
    }

    void Attack(string tag){

        // Debug.Log("We are attacking");

        if (tag == "Flamelet"){

            EnemyDash dash = GetComponent<EnemyDash>();
            attackTimer = attackCooldown;

            if (dash != null){

                dash.Dash();
                currentState = EnemyState.stDuringAttack;
                // After Attack
                StartCoroutine(AttackCooldown());
            } 
        
            else Debug.LogWarning("Come on, man. You don't got that dash script!");

        }
        else if (tag == "Brute"){

            // return;

        }

        else if (tag == "Boomba"){

            // return;

        }
        
        

    }




}
