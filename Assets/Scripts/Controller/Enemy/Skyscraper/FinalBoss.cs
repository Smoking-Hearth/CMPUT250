using UnityEngine;
using UnityEngine.Events;

public class FinalBoss : MonoBehaviour
{
    private enum FinalBossState
    {
        INACTIVE, STANDBY, PUNCH, RAIN, SPAWN
    }
    private FinalBossState state;

    [Min(1)]
    [SerializeField] private float standByDuration = 1;
    private float standbyTimer;

    [SerializeField] private float baseAltitude;
    [SerializeField] private float floorHeight;
    private float buildingHeight;
    [Min(1)]
    [SerializeField] private int floorCount;
    private FinalBossFloor[] floors;
    private int currentFloor;

    [SerializeField] private GameObject[] spawnObjects;
    [SerializeField] private FinalBossFloor groundFloor;
    [SerializeField] private FinalBossFloor topFloor;
    [SerializeField] private FinalBossFloor[] floorPrefabs;

    [SerializeField] private UnityEvent completeEvent;

    [Header("Projectiles")]
    private EnemyProjectile[] cache;
    private int currentProjectileIndex;


    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        floors = new FinalBossFloor[floorCount];
        floors[0] = Instantiate(groundFloor, transform.position + new Vector3(0, baseAltitude), Quaternion.identity, transform);
        floors[floorCount - 1] = Instantiate(topFloor, transform.position + new Vector3(0, baseAltitude + floorCount * floorHeight), Quaternion.identity, transform);
        for (int i = 1; i < floorCount - 1; i++)
        {
            int randomFloor = Random.Range(0, floorPrefabs.Length);
            floors[i] = Instantiate(floorPrefabs[randomFloor], transform.position + new Vector3(0, baseAltitude + i * floorHeight), Quaternion.identity, transform);
        }
        buildingHeight = floorHeight * floors.Length;
    }

    public void Activate(bool active)
    {
        if (active)
        {
            state = FinalBossState.STANDBY;
        }
        else
        {
            state = FinalBossState.INACTIVE;
        }
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case FinalBossState.INACTIVE:
                break;
            case FinalBossState.STANDBY:
                StandbyState();
                break;
            case FinalBossState.PUNCH:
                PunchAttack();
                break;
            case FinalBossState.RAIN:
                RainAttack();
                break;
            case FinalBossState.SPAWN:
                SpawnState();
                break;
        }
    }

    private void CheckCurrentFloor()
    {
        float playerY = gameObject.MyLevelManager().Player.Position.y - baseAltitude;
        currentFloor = Mathf.Clamp(Mathf.CeilToInt(playerY / buildingHeight * floorCount), 0, floorCount);
    }

    private void StandbyState()
    {
        CheckCurrentFloor();

        if (currentFloor >= floorCount - 1)
        {
            completeEvent.Invoke();
            return;
        }

        if (standbyTimer > 0)
        {
            standbyTimer -= Time.fixedDeltaTime;

            if (Random.Range(0, 1000) == 0)
            {
                floors[currentFloor].SendDrone();
            }
        }
        else
        {
            int randomState = Random.Range(0, 3);

            switch (randomState)
            {
                case 0:
                    state = FinalBossState.PUNCH;
                    break;
                case 1:
                    state = FinalBossState.RAIN;
                    break;
                case 2:
                    state = FinalBossState.SPAWN;
                    break;
            }

            standbyTimer = standByDuration;
        }
    }
    private void PunchAttack()
    {
        int random = Random.Range(0, 2);
        if (currentFloor + random < floorCount)
        {
            floors[currentFloor + random].ActivateArm();
        }
        state = FinalBossState.STANDBY;
    }
    private void RainAttack()
    {
        state = FinalBossState.STANDBY;
    }

    private void SpawnState()
    {
        int randomSpawn = Random.Range(0, spawnObjects.Length);
        int randomFloor = currentFloor + Random.Range(-1, 3);

        if (randomFloor >= floors.Length || randomFloor < 0)
        {
            return;
        }

        if (!floors[randomFloor].LoadDoor(spawnObjects[randomSpawn]))
        {
            standbyTimer = 0;
        }

        state = FinalBossState.STANDBY;
    }
}
