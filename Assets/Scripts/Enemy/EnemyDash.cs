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
    public float DashSpeed = 3f;

    public float cooldown = 1f;
    //public float dashSpeed = 5f;

    //private Position playerPosition = GameObject.FindGameObjectWithTag("Player").GetComponent<>(); 

    public void Dash(){

        //Debug.Log("Began to dash");

        // Save To and From positions
        targetPosition = player.transform.position;
        enemyPosition = enemy.transform.position;
        flameSprite = GetComponent<SpriteRenderer>();
        oldColor = flameSprite.color;

        //Debug.Log("oldColor:" + oldColor);
        // flameSprite.color = newColor;

        //Debug.Log("Player saved position:" + targetPosition);
        //Debug.Log("Enemy current position" + enemyPosition);

        StartCoroutine(BeginToDash(targetPosition, DashSpeed));
    }

    public IEnumerator BeginToDash(Vector3 target, float seconds)
    {   

        //Debug.Log("Cannot Damage");
        GetComponent<EnemyController>().cannotDamage = true;

        //Debug.Log("Changed colour");
        flameSprite.color = newColor;

        Vector3 direction = (targetPosition - enemyPosition).normalized;
        Vector3 finalPlace = enemyPosition + direction;

        float overallTime = 0f;

        //yield return new WaitForSeconds(cooldown);

        while (overallTime < seconds){

        float dashingTime = overallTime / seconds;  // Change target position to past player
        enemy.transform.position = Vector3.Lerp(enemyPosition, targetPosition, dashingTime);
        overallTime += Time.deltaTime;
        //Debug.Log("Player saved position:" + targetPosition);
        //Debug.Log("Enemy current position" + enemyPosition);

        yield return null;
        }

        // while (Vector3.Distance(enemy.transform.position, targetPosition) >= 0.5f){
        //     enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, finalPlace, DashSpeed * Time.deltaTime);
        //     yield return null;
        // }

        // yield return new WaitForSeconds(cooldown);
        // Not working
        
        // GetComponent<EnemyController>().cannotDamage = false;
        GetComponent<EnemyController>().currentState = EnemyController.EnemyState.stTargeting;
        //Debug.Log("No longer invincible");


        


        // Make enemy stop in place
        // Get position of player, save to variable
        // Wait a second or two
        // make enemy dash to position quickly
        // Time out movement for second
        // Resume movement, cannot Damage = false

    }


}
