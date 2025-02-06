using UnityEngine;

public class FinalBoss : MonoBehaviour
{
    private enum FinalBossState
    {
        INACTIVE, STANDBY, PUNCH, COLUMN, SPAWN
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
    [SerializeField] private FinalBossFloor floorPrefab;

    public void Generate()
    {
        floors = new FinalBossFloor[floorCount];
        for (int i = 0; i < floorCount; i++)
        {
            floors[i] = Instantiate(floorPrefab, transform.position + new Vector3(0, baseAltitude + i * floorHeight), Quaternion.identity, transform);
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
            case FinalBossState.COLUMN:
                ColumnAttack();
                break;
            case FinalBossState.SPAWN:
                SpawnState();
                break;
        }
    }

    private void CheckCurrentFloor()
    {
        float playerY = GameManager.PlayerPosition.y - baseAltitude;
        currentFloor = Mathf.Clamp(Mathf.CeilToInt(playerY / buildingHeight * floorCount), 0, floorCount);
    }

    private void StandbyState()
    {
        CheckCurrentFloor();

        if (currentFloor >= floorCount - 1)
        {
            return;
        }

        if (standbyTimer > 0)
        {
            standbyTimer -= Time.fixedDeltaTime;
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
                    state = FinalBossState.COLUMN;
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
        state = FinalBossState.STANDBY;

    }
    private void ColumnAttack()
    {
        state = FinalBossState.STANDBY;
    }

    private void SpawnState()
    {
        int randomSpawn = Random.Range(0, spawnObjects.Length);

        if (!floors[currentFloor + 1].LoadDoor(spawnObjects[randomSpawn]))
        {
            standbyTimer = 0;
        }

        state = FinalBossState.STANDBY;
    }
}
