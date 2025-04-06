using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

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
        public StaircaseSection rightStaircase;
        public ConnectorFloor connector;
        public StaircaseSection leftStaircase;

        public BossFloor(StaircaseSection right, ConnectorFloor connect, StaircaseSection left)
        {
            rightStaircase = right;
            connector = connect;
            leftStaircase = left;
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

    public float CurrentFloorLevel
    {
        get
        {
            return transform.position.y + baseAltitude + currentFloor * floorHeight;
        }
    }

    [SerializeField] private FloorConnectorSorter sorter;
    [SerializeField] private GameObject[] spawnObjects;
    [SerializeField] private BossFloor groundFloor;
    [SerializeField] private BossFloor topFloor;

    [Header("Stairs")]
    [SerializeField] private StaircaseSection[] rightStaircasePrefabs;
    [SerializeField] private StaircaseSection[] leftStaircasePrefabs;
    [SerializeField] private StaircaseSection[] rightUnclimbable;
    [SerializeField] private StaircaseSection[] leftUnclimbable;

    [Header("Connectors")]
    [SerializeField] private ConnectorFloor[] connectorPrefabs;
    [SerializeField] private ConnectorFloor activateBossConnector;
    [SerializeField] private List<int> fullPassageFloor = new List<int>();    //Floors that will need to have a full connecter through them
    [SerializeField] private int activateBossFloor;

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

        //Create first floor
        StaircaseSection rightInitial = Instantiate(groundFloor.rightStaircase, transform.position + new Vector3(0, baseAltitude), Quaternion.identity, transform);
        ConnectorFloor connectInitial = Instantiate(groundFloor.connector, transform.position + new Vector3(0, baseAltitude), Quaternion.identity, transform);
        StaircaseSection leftInitial = Instantiate(groundFloor.leftStaircase, transform.position + new Vector3(-60, baseAltitude), Quaternion.identity, transform);

        //Create top floor
        StaircaseSection rightEnd = Instantiate(topFloor.rightStaircase, transform.position + new Vector3(0, baseAltitude + (floorCount - 1) * floorHeight), Quaternion.identity, transform);
        ConnectorFloor connectEnd = Instantiate(topFloor.connector, transform.position + new Vector3(0, baseAltitude + (floorCount - 1) * floorHeight), Quaternion.identity, transform);
        StaircaseSection leftEnd = Instantiate(topFloor.leftStaircase, transform.position + new Vector3(-60, baseAltitude + (floorCount - 1) * floorHeight), Quaternion.identity, transform);

        floors[0] = new BossFloor(rightInitial, connectInitial, leftInitial);
        floors[floorCount - 1] = new BossFloor(rightEnd, connectEnd, leftEnd);

        bool rightSide = true;
        for (int i = 1; i < floorCount - 1; i++)
        {
            int rightIndex = Random.Range(0, rightStaircasePrefabs.Length);
            int lefIndex = Random.Range(0, leftStaircasePrefabs.Length);
            ConnectorFloor.Connections connections = ConnectorFloor.Connections.RIGHT;
            ConnectorFloor.Connections exclude = new ConnectorFloor.Connections();

            StaircaseSection right = rightStaircasePrefabs[rightIndex];
            StaircaseSection left = leftStaircasePrefabs[lefIndex];


            //Checking if the previous floor connects to this one
            if ((floors[i - 1].connector.floorConnections & ConnectorFloor.Connections.UP) > 0)
            {
                connections |= ConnectorFloor.Connections.DOWN;
            }

            
            if (fullPassageFloor.Contains(i + 1))
            {
                exclude |= ConnectorFloor.Connections.UP;
            }
            if (fullPassageFloor.Contains(i))
            {
                connections |= ConnectorFloor.Connections.LEFT | ConnectorFloor.Connections.RIGHT;
                exclude |= ConnectorFloor.Connections.DOWN;
                if (rightSide)
                {
                    right = rightUnclimbable[0];
                }
                else
                {
                    left = leftUnclimbable[0];
                }
            }
            if (fullPassageFloor.Contains(i - 1))
            {
                exclude |= ConnectorFloor.Connections.UP;
                if (rightSide)
                {
                    right = rightUnclimbable[1];
                }
                else
                {
                    left = leftUnclimbable[1];
                }
                rightSide = !rightSide;
            }


            right = Instantiate(right, transform.position + new Vector3(0, baseAltitude + i * floorHeight), Quaternion.identity, transform);
            left = Instantiate(left, transform.position + new Vector3(-60, baseAltitude + i * floorHeight), Quaternion.identity, transform);
            ConnectorFloor connector = Instantiate(sorter.GetFittingConnector(connections, exclude), transform.position + new Vector3(0, baseAltitude + i * floorHeight), Quaternion.identity, transform);
            floors[i] = new BossFloor(right, connector, left);
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

        /*if (!floors[randomFloor].rightStaircase.LoadDoor(spawnObjects[randomSpawn]))
        {
            standbyTimer = 0;
        }*/

        state = FinalBossState.STANDBY;
    }
}
