using UnityEngine;

public class ConnectorFloor : MonoBehaviour
{
    [System.Flags]
    public enum Connections
    {
        UP = 1 << 0,
        DOWN = 1 << 1,
        LEFT = 1 << 2,
        RIGHT = 1 << 3,
    }

    [System.Serializable]
    private struct ConnectorFloorSpawn
    {
        public Transform spawnPoint;
        [Range(0, 1)]
        public float successRate;
        public EnemyController[] enemyPool;
    }

    [SerializeField] private ConnectorFloorSpawn[] enemySpawns;
    public Connections floorConnections;
    [SerializeField] private Door[] doors;

    public bool Spawned { get; private set; }

    public void SpawnEnemies()
    {
        for (int i = 0; i < enemySpawns.Length; i++)
        {
            if (Random.Range(0f, 1f) <= enemySpawns[i].successRate)
            {
                int randomEnemy = Random.Range(0, enemySpawns[i].enemyPool.Length);
                Instantiate(enemySpawns[i].enemyPool[randomEnemy], enemySpawns[i].spawnPoint.position, Quaternion.identity, transform);
            }
        }

        Spawned = true;
    }

    public void OpenDoors()
    {
        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].Open();
        }
    }
}
