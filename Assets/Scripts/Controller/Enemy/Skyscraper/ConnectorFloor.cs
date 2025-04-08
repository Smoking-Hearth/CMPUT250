using UnityEngine;
using System.Collections.Generic;

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
        public Transform spawnEffect;
    }

    [SerializeField] private ConnectorFloorSpawn[] enemySpawns;
    private List<EnemyController> spawnedEnemies = new List<EnemyController>();
    public Connections floorConnections;
    [SerializeField] private Door[] doors;
    [SerializeField] private float spawnDelay;
    private float spawnTimer;

    public bool Spawned { get; set; }

    private void FixedUpdate()
    {
        if (spawnTimer > 0)
        {
            spawnTimer -= Time.fixedDeltaTime;

            if (spawnTimer <= 0)
            {
                SpawnEnemies();
            }
        }
    }

    public void InitiateSpawn()
    {
        spawnTimer = spawnDelay;
        for (int i = 0; i < enemySpawns.Length; i++)
        {
            if (Random.Range(0f, 1f) <= enemySpawns[i].successRate)
            {
                int randomEnemy = Random.Range(0, enemySpawns[i].enemyPool.Length);
                Instantiate(enemySpawns[i].spawnEffect, enemySpawns[i].spawnPoint.position, Quaternion.identity, transform);
                EnemyController enemy = Instantiate(enemySpawns[i].enemyPool[randomEnemy], enemySpawns[i].spawnPoint.position, Quaternion.identity, transform);
                enemy.gameObject.SetActive(false);

                spawnedEnemies.Add(enemy);
            }
        }

        Spawned = true;
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            spawnedEnemies[i].gameObject.SetActive(true);
        }
    }

    public void OpenDoors()
    {
        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].Open();
        }
    }
}
