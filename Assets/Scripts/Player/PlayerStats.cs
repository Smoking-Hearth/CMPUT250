using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum DamageType
{
    Fire, Electricity
}

[RequireComponent(typeof(PlayerController))]
public class PlayerStats : MonoBehaviour
{
    private Health health;
    [SerializeField] private Slider healthBar;

    [SerializeField] private float hurtRadius;
    private PlayerController controller;
    private PlayerSounds sounds;

    void Awake()
    {
        health = GetComponent<Health>();
        healthBar.maxValue = health.Max;
        healthBar.minValue = health.Min;
        healthBar.value = health.Current;

        controller = GetComponent<PlayerController>();
        sounds = controller.Sounds;
    }

    private void OnEnable()
    {
        health.onChanged += UpdateHealthBar;
        GameManager.onEnemyAttack += CheckEnemyAttack;
    }

    private void OnDisable()
    {
        health.onChanged -= UpdateHealthBar;
        GameManager.onEnemyAttack -= CheckEnemyAttack;
    }

    void UpdateHealthBar()
    {
        healthBar.value = health.Current;

        if (healthBar.value <= 0f  || transform.position.y <= -40f){  //PROBLEM: Doesn't respawn you when you fall off edge, but may not matter
            OnDeath();
        }
    }

    void OnDeath(){
        LevelManager.Active.levelState = LevelState.Defeat;
    }


    private void CheckEnemyAttack(Vector2 position, Vector2 sourcePosition, EnemyAttackInfo attackInfo)
    {
        if (GameManager.levelState != LevelState.Playing)
        {
            return;
        }

        Vector2 attackDirection = (Vector2)transform.position - position;
        float attackDistance = attackDirection.magnitude;

        Vector2 directionFromSource = (Vector2)transform.position - sourcePosition;
        if (attackDistance < hurtRadius + attackInfo.radius)
        {
            health.Current -= attackInfo.damage;
            sounds.PlayHurt(attackInfo.damageType);

            float closeness = Mathf.Clamp01(1 - attackDistance / (attackInfo.radius + hurtRadius));
            Vector2 knockback = new Vector2(closeness * directionFromSource.normalized.x * attackInfo.knockbackPower.x, attackInfo.knockbackPower.y);
            controller.PushPlayer(knockback);
        }
    }
}
