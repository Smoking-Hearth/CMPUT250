using UnityEngine;
using UnityEngine.Events;

public class FinalBoss : MonoBehaviour
{
    private enum FinalBossState
    {
        INACTIVE, STANDBY, PUNCH, RAIN, SPAWN
    }
    private FinalBossState state;

    [System.Serializable]
    private struct BossFloor
    {
        public FinalBossFloor rightStaircase;
        public ConnectorFloor connector;

        public BossFloor(FinalBossFloor right, ConnectorFloor connect)
        {
            rightStaircase = right;
            connector = connect;
        }
    }

    [Min(1)]
    [SerializeField] private float standByDuration = 1;
    private float standbyTimer;

    [SerializeField] private float baseAltitude;
    [SerializeField] private float floorHeight;
    private float buildingHeight;
    [Min(1)]
    [SerializeField] private int floorCount;
    private BossFloor[] floors;
    private int currentFloor;

    [SerializeField] private FloorConnectorSorter sorter;
    [SerializeField] private GameObject[] spawnObjects;
    [SerializeField] private BossFloor groundFloor;
    [SerializeField] private BossFloor topFloor;
    [SerializeField] private FinalBossFloor[] staircasePrefabs;
    [SerializeField] private ConnectorFloor[] connectorPrefabs;

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
        sorter.Sort(connectorPrefabs);
        floors = new BossFloor[floorCount];
        FinalBossFloor rightInitial = Instantiate(groundFloor.rightStaircase, transform.position + new Vector3(0, baseAltitude), Quaternion.identity, transform);
        ConnectorFloor connectInitial = Instantiate(groundFloor.connector, transform.position + new Vector3(0, baseAltitude), Quaternion.identity, transform);
        FinalBossFloor rightEnd = Instantiate(topFloor.rightStaircase, transform.position + new Vector3(0, baseAltitude + floorCount * floorHeight), Quaternion.identity, transform);
        ConnectorFloor connectEnd = Instantiate(topFloor.connector, transform.position + new Vector3(0, baseAltitude + floorCount * floorHeight), Quaternion.identity, transform);
        floors[0] = new BossFloor(rightInitial, connectInitial);
        floors[floorCount - 1] = new BossFloor(rightEnd, connectEnd);
        for (int i = 1; i < floorCount - 1; i++)
        {
            int rightIndex = Random.Range(0, staircasePrefabs.Length);
            ConnectorFloor.Connections connections = ConnectorFloor.Connections.RIGHT;

            //Checking if the previous floor connects to this one
            if ((floors[i - 1].connector.floorConnections & ConnectorFloor.Connections.UP) > 0)
            {
                connections |= ConnectorFloor.Connections.DOWN;
            }
            FinalBossFloor right = Instantiate(staircasePrefabs[rightIndex], transform.position + new Vector3(0, baseAltitude + i * floorHeight), Quaternion.identity, transform);
            ConnectorFloor connector = Instantiate(sorter.GetFittingConnector(connections), transform.position + new Vector3(0, baseAltitude + i * floorHeight), Quaternion.identity, transform);
            floors[i] = new BossFloor(right, connector);
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
                floors[currentFloor].rightStaircase.SendDrone();
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
            floors[currentFloor + random].rightStaircase.ActivateArm();
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

        if (!floors[randomFloor].rightStaircase.LoadDoor(spawnObjects[randomSpawn]))
        {
            standbyTimer = 0;
        }

        state = FinalBossState.STANDBY;
    }
}
