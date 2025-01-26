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
    public float DashSpeed = 0.5f;

    public float cooldown = 0.5f;
    //public float dashSpeed = 5f;

    //private Position playerPosition = GameObject.FindGameObjectWithTag("Player").GetComponent<>(); 

    public void Dash(){

        //Debug.Log("Began to dash");

        // Save To and From positions
        targetPosition = player.transform.position;
        enemyPosition = enemy.transform.position;
        flameSprite = GetComponent<SpriteRenderer>();
        oldColor = flameSprite.color;
        // flameSprite.color = newColor;

        //Debug.Log("Player saved position:" + targetPosition);
        //Debug.Log("Enemy current position" + enemyPosition);

        StartCoroutine(BeginToDash(targetPosition, DashSpeed));
    }

    public IEnumerator BeginToDash(Vector3 target, float seconds)
    {   

        Debug.Log("Cannot Damage");
        GetComponent<EnemyController>().cannotDamage = true;

        Debug.Log("Changed colour");
        flameSprite.color = newColor;


        float overallTime = 0f;
        //Vector3 startPosition = transform.position;


        //Vector3 direction = (target.position - startPosition).normalized;


        //GetComponent<EnemyController>().canMove = true;

        yield return new WaitForSeconds(cooldown);
        while (overallTime < seconds){

    
        float dashingTime = overallTime / seconds;
        Debug.Log("We are currently dashing");
        enemy.transform.position = Vector3.Lerp(enemyPosition, targetPosition, dashingTime);
        overallTime += Time.deltaTime;
        //Debug.Log("Player saved position:" + targetPosition);
        //Debug.Log("Enemy current position" + enemyPosition);

        yield return null;
        }

        GetComponent<EnemyController>().cannotDamage = false;
        Debug.Log("No longer invincible");

        // Back to normal sprite
        flameSprite.color = oldColor;
        Debug.Log("Back to normal sprite");

        yield return new WaitForSeconds(cooldown);
        GetComponent<EnemyController>().canMove = true;

        // Make enemy stop in place
        // Get position of player, save to variable
        // Wait a second or two
        // make enemy dash to position quickly
        // Time out movement for second
        // Resume movement, cannot Damage = false

    }


}
