using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyDash : MonoBehaviour
{
    private SpriteRenderer flameSprite;
    private Color oldColor;
    public Color newColor;

    public GameObject player;

    public GameObject enemy;

    private Vector3 targetPosition;
    private Vector3 enemyPosition;
    public float DashSpeed = 18f;
    public float timeDashing = 0.5f;

    public float cooldown = 0.5f;

    public System.Action OnDashComplete;

    
    // STEPS
    // Make enemy stop in place before attacking & make them invincible
    // Get position of player, save to variable
    // make enemy dash to position quickly
    // Resume movement, cannot Damage = false
    // Go on cooldown

    public void Dash(){

        // Save To and From positions
        targetPosition = player.transform.position;
        enemyPosition = enemy.transform.position;
        flameSprite = GetComponent<SpriteRenderer>();
        oldColor = flameSprite.color;

        StartCoroutine(BeginToDash(targetPosition, timeDashing));
    }

    public IEnumerator BeginToDash(Vector3 target, float time)
    {   

        GetComponent<EnemyController>().cannotDamage = true;
        GetComponent<EnemyController>().canMove = false;

        flameSprite.color = newColor;

        Vector3 direction = (targetPosition - enemyPosition).normalized;

        float overallTime = 0f;

        // Stop moving before dashing
        yield return new WaitForSeconds(cooldown);

        while (overallTime < time){

        // float dashingTime = overallTime / seconds;  // Change target position to past player
        enemy.transform.Translate(DashSpeed * Time.deltaTime * direction);
        //enemy.transform.position = Vector3.Lerp(enemyPosition, targetPosition, dashingTime);
        overallTime += Time.deltaTime;
        yield return null;
        }

        // Return back to normal colour
        flameSprite.color = oldColor;

        // Resume previous movement
        GetComponent<EnemyController>().cannotDamage = false;
        GetComponent<EnemyController>().canMove = true;


        OnDashComplete?.Invoke();

    }


}
