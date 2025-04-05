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
        public Vector2 position;
        [Range(0, 1)]
        public float successRate;
        public EnemyController[] enemyPool;
    }

    [SerializeField] private ConnectorFloorSpawn[] enemySpawns;
    public Connections floorConnections;

    public void SpawnEnemies()
    {
        for (int i = 0; i < enemySpawns.Length; i++)
        {
            if (Random.Range(0f, 1f) <= enemySpawns[i].successRate)
            {
                int randomEnemy = Random.Range(0, enemySpawns[i].enemyPool.Length);
                Instantiate(enemySpawns[i].enemyPool[randomEnemy], (Vector2)transform.position + enemySpawns[i].position, Quaternion.identity, transform);
            }
        }
    }
}
