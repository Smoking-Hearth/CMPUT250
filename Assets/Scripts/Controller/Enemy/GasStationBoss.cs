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
    [SerializeField] private float airSpeed;
    [SerializeField] private float airDuration;
    [SerializeField] private Rigidbody2D airRigidbody;
    [SerializeField] private GameObject[] crucibles;
    private Vector2[] crucibleSpawns;
    private float airTimer;

    [SerializeField] private float projectileHeight;
    [SerializeField] private int burstAmount;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private int cacheCapacity;
    [SerializeField] private float shootInterval;
    private EnemyProjectile[] cache;
    private int currentProjectileIndex;
    private float shootTimer;

    [SerializeField] private UnityEvent completeEvent;

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
                break;
        }
    }

    public void ReturnToAir()
    {
        airRigidbody.gameObject.SetActive(true);
        airTimer = airDuration;
        shootTimer = shootInterval;
        currentState = GasStationBossState.Air;
        airRigidbody.linearVelocityX = airSpeed;
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
        int randomState = Random.Range(0, 1);

        switch (randomState)
        {
            case 0:
                SpawnCrucibles();
                currentState = GasStationBossState.Crucible;
                break;
            case 1:
                currentState = GasStationBossState.Trailblazer;
                break;
        }
    }

    private void CrucibleState()
    {
        
    }

    private void SpawnCrucibles()
    {
        for (int i = 0; i < crucibles.Length; i++)
        {
            crucibles[i].transform.position = crucibleSpawns[i];
            crucibles[i].SetActive(true);
        }
    }

    public void Complete()
    {
        completeEvent.Invoke();
        currentState = GasStationBossState.Inactive;
    }
}
