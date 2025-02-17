using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    public CombustibleKind fireKind;

    [Header("Attacking")]
    public EnemyAttackInfo attackInfo;
    public Transform attackPrefab; //PLACEHOLDER
    public float aggroRange = 5f;
    public float attackRange = 2f;
    public float commitAttackSeconds = 1.5f;
    public float frontSwingSeconds;
    public float backSwingSeconds;


    [Header("Health")]
    public float maxHealth;
    public float defeatDurationSeconds;
    public float blinkDuration;
    public int blinkFrequency;
    public Color normalColor;
    public Color blinkColor;

    [Header("Movement")]
    public float speed;

    [Header("Particles")]
    public ParticleSystem spawnParticles;
    public ParticleSystem hurtParticles;
    public ParticleSystem backfireParticles;

    [Header("Sounds")]
    public AudioClip hurtClip;
    public AudioClip attackClip;
    public AudioClip extinguishClip;
}
