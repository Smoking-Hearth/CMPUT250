using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private List<EnemyController> storedEnemies = new List<EnemyController>();
    [SerializeField] private List<EnemyController> spawnedEnemies = new List<EnemyController>();
    [Tooltip("Should enemies be positioned when spawned?")]
    [SerializeField] private bool positionEnemies;
    [SerializeField] private Vector2 spawnArea;
    [SerializeField] private float spawnInterval;
    private float spawnTimer;
    [Tooltip("Does the spawner respawn defeated enemies?")]
    [SerializeField] private bool continuous;
    [SerializeField] private bool activated;

    void FixedUpdate()
    {
        if (!activated)
        {
            return;
        }
        if (spawnTimer > 0)
        {
            if (storedEnemies.Count > 0)
            {
                spawnTimer -= Time.fixedDeltaTime;
            }
        }
        else
        {
            spawnTimer = spawnInterval;
            if (continuous)
            {
                for (int i = 0; i < spawnedEnemies.Count; i++)
                {
                    if (!spawnedEnemies[i].gameObject.activeSelf)
                    {
                        storedEnemies.Add(spawnedEnemies[i]);
                        spawnedEnemies.RemoveAt(i);
                    }
                }
            }

            if (storedEnemies.Count > 0)
            {
                int random = Random.Range(0, storedEnemies.Count);
                SpawnEnemy(random);
            }
        }
    }

    private void SpawnEnemy(int index)
    {
        if (positionEnemies)
        {
            Vector2 randomPosition = new Vector2(Random.Range(-spawnArea.x, spawnArea.x), Random.Range(-spawnArea.y, spawnArea.y));
            storedEnemies[index].transform.position = (Vector2)transform.position + randomPosition;
        }

        storedEnemies[index].gameObject.SetActive(true);

        spawnedEnemies.Add(storedEnemies[index]);
        storedEnemies.RemoveAt(index);
    }

    public void Activate(bool set)
    {
        activated = set;
    }
}
