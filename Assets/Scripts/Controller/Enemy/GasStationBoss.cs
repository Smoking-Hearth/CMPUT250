using UnityEngine;
using UnityEngine.Events;

public class GasStationBoss : MonoBehaviour
{
    public enum GasStationBossState
    {
        Inactive, Air, Crucible, Trailblazer
    }
    private GasStationBossState currentState;
    [SerializeField] private float arenaExtents;
    [SerializeField] private Animator animator;

    [SerializeField] private EnemyHealth healthComponent;
    [SerializeField] private FireSounds sounds;
    [SerializeField] private float defeatDurationSeconds;

    [Header("Crucible State")]
    [SerializeField] private GameObject[] crucibles;
    private Vector2[] crucibleSpawns;

    [Header("Air State")]
    [SerializeField] private float projectileHeight;
    [SerializeField] private int burstAmount;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private int cacheCapacity;
    [SerializeField] private float shootInterval;
    private EnemyProjectile[] cache;
    private int currentProjectileIndex;
    private float shootTimer;
    [SerializeField] private float airSpeed;
    [SerializeField] private float airDuration;
    [SerializeField] private Rigidbody2D airRigidbody;
    private float airTimer;

    [Header("Trailblazer State")]
    [SerializeField] private PathFollower trailblazer;
    [SerializeField] private int tbRepetitions;
    [SerializeField] private Path tbPathTop;
    [SerializeField] private Path tbPathBottom;
    [SerializeField] private Transform tbWarningTop;
    [SerializeField] private Transform tbWarningBottom;
    [SerializeField] private float tbDelay;
    [SerializeField] private float tbSweepDuration;
    private float tbTimer;
    private float tbCounter;

    [Header("Extinguisher")]
    [SerializeField] private DroneDropItem extinguisherDronePrefab;
    [SerializeField] private float extinguisherDropInterval;
    private float extinguisherDropTimer;

    [SerializeField] private UnityEvent completeEvent;

    private void OnEnable()
    {
        healthComponent.onHurt += Hurt;
    }
    private void OnDisable()
    {
        healthComponent.onHurt -= Hurt;
    }
    public void Activate()
    {
        cache = new EnemyProjectile[cacheCapacity];
        crucibleSpawns = new Vector2[crucibles.Length];

        for (int i = 0; i < crucibles.Length; i++)
        {
            crucibleSpawns[i] = crucibles[i].transform.position;
        }

        currentState = GasStationBossState.Air;
        ReturnToAir();
    }
    private void FixedUpdate()
    {
        switch(currentState)
        {
            case GasStationBossState.Inactive:
                break;
            case GasStationBossState.Air:
                AirState();
                break;
            case GasStationBossState.Crucible:
                CrucibleState();
                break;
            case GasStationBossState.Trailblazer:
                TrailblazerState();
                break;
        }
    }

    public void ExtinguisherTimer()
    {
        if (extinguisherDropInterval > 0)
        {
            extinguisherDropTimer -= Time.fixedDeltaTime;

            if (extinguisherDropTimer <= 0)
            {
                SpawnExtinguisherDrop();
                extinguisherDropTimer = extinguisherDropInterval;
            }
        }
    }

    public void ReturnToAir()
    {
        airRigidbody.gameObject.SetActive(true);
        airTimer = airDuration;
        shootTimer = shootInterval;
        currentState = GasStationBossState.Air;
        airRigidbody.linearVelocityX = airSpeed;
        animator.SetTrigger("Rise");
    }

    private void AirState()
    {
        if (airRigidbody.position.x < transform.position.x - arenaExtents)
        {
            airRigidbody.linearVelocityX = airSpeed;
        }
        else if (airRigidbody.position.x > transform.position.x + arenaExtents)
        {
            airRigidbody.linearVelocityX = -airSpeed;
        }

        if (shootTimer > 0)
        {
            shootTimer -= Time.fixedDeltaTime;

            if (shootTimer <= 0)
            {
                shootTimer = shootInterval;
                SpawnProjectiles();
            }
        }

        if (airTimer > 0)
        {
            airTimer -= Time.fixedDeltaTime;

            if (airTimer <= 0)
            {
                ChooseState();
            }
        }
    }

    private void SpawnProjectiles()
    {
        for (int i = 0; i < burstAmount; i++)
        {
            Vector2 randomPosition = (Vector2)transform.position + new Vector2(Random.Range(-arenaExtents, arenaExtents), projectileHeight);
            if (cache[currentProjectileIndex] == null)
            {
                cache[currentProjectileIndex] = Instantiate(projectilePrefab, randomPosition, Quaternion.identity, null);
            }
            else
            {
                cache[currentProjectileIndex].ResetProjectile();
                cache[currentProjectileIndex].transform.position = randomPosition;
            }
            currentProjectileIndex = (currentProjectileIndex + 1) % cacheCapacity;
        }
    }

    private void ChooseState()
    {
        airRigidbody.linearVelocityX = 0;
        airRigidbody.gameObject.SetActive(false);
        int randomState = Random.Range(0, 2);

        switch (randomState)
        {
            case 0:
                SpawnCrucibles();
                extinguisherDropTimer = extinguisherDropInterval;
                currentState = GasStationBossState.Crucible;
                break;
            case 1:
                currentState = GasStationBossState.Trailblazer;
                tbCounter = 0;
                tbTimer = tbDelay;
                break;
        }
    }

    private void CrucibleState()
    {
        ExtinguisherTimer();
    }

    private void SpawnCrucibles()
    {
        for (int i = 0; i < crucibles.Length; i++)
        {
            crucibles[i].transform.position = crucibleSpawns[i];
            crucibles[i].SetActive(true);
        }
    }

    public void SpawnExtinguisherDrop()
    {
        int sideDecision = -1 + 2 * Random.Range(0, 2);
        Vector2 spawnPosition = new Vector2(transform.position.x + arenaExtents * 1.5f * sideDecision, transform.position.y);
        Instantiate(extinguisherDronePrefab, spawnPosition, Quaternion.identity, transform);
    }

    private void TrailblazerState()
    {
        if (tbTimer > 0)
        {
            tbTimer -= Time.fixedDeltaTime;
            if (tbTimer <= 0)
            {
                if (tbCounter >= tbRepetitions)
                {
                    ReturnToAir();
                    return;
                }

                SendTrailblazer();
                tbTimer = tbDelay + tbSweepDuration;
            }
        }
    }

    private void SendTrailblazer()
    {
        int sideDecision = Random.Range(0, 2);

        switch (sideDecision)
        {
            case 0:
                trailblazer.transform.position = tbPathTop.GetNodePosition(0);
                tbPathTop.AddFollower(trailblazer);
                break;
            case 1:
                trailblazer.transform.position = tbPathBottom.GetNodePosition(0);
                tbPathBottom.AddFollower(trailblazer);
                break;
        }

        tbCounter++;
    }
    public virtual void Hurt()
    {
        if (healthComponent.HealthZero)
        {
            //currentState = EnemyState.stDefeated;
            //defeatTimer = enemyInfo.defeatDurationSeconds;
            sounds.ExtinguishSound();
            sounds.FadeAmbientSounds(defeatDurationSeconds);
            currentState = GasStationBossState.Inactive;
            //enemyAnimator.SetTrigger("IsDead");
        }
        else
        {
            sounds.HitSound();
        }
    }

    public void Complete()
    {
        completeEvent.Invoke();
    }
}
