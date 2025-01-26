using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : MonoBehaviour 
{
    static int waterLayer;
    private Transform target;
    public float speed;
    private float distance;
    // private bool inRange = false;
    public bool cannotDamage = false;
    public bool canMove = true;
    public float maxRange = 10f;

    public float attackTimer = 0f;
    public float attackCooldown = 1.5f;

    //public ScriptableObject FlameDash;

    void Awake() 
    {
        waterLayer = LayerMask.NameToLayer("Water");
        GetComponent<Health>().OnDepleted += () => {
            Destroy(gameObject);
        };
        
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

    //BUG: Character falls slowly when using 2D colliders
    // void OnTriggerEnter2D(Collider2D Collision){

    //     if(Collision.CompareTag("Player")){

    //         inRange = true;
    //     }
    // }
    // void OnTriggerExit2D(Collider2D Collision){

    //     if(Collision.CompareTag("Player")){

    //         inRange = false;
    //     }
    // }

    void Update(){

        distance = Vector2.Distance(transform.position, target.position);

        // if (inRange)
        if (distance < maxRange && canMove){
        //Debug.Log("We are currently moving");
        transform.position = Vector2.MoveTowards(transform.position, target.position, Time.deltaTime * speed);
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown){

            canMove = false;
        
            if (GameObject.FindGameObjectWithTag("Flamelet")){
                //Debug.Log("We called dash");
                GetComponent<EnemyDash>().Dash();
                attackTimer = 0;

        }

        }


        }

    }

}