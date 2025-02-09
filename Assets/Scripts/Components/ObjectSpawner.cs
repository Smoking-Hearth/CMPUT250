using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPrefab;
    [Range(1, 20)]
    [SerializeField] private int maxObjects;
    [SerializeField] private Vector2 spawnPosition;
    private Transform[] spawnedObjects;
    private int currentIndex;

    void Start()
    {
        spawnedObjects = new Transform[maxObjects];
    }

    public void Spawn()
    {
        currentIndex = (currentIndex + 1) % maxObjects;
        if (spawnedObjects[currentIndex] != null)
        {
            Destroy(spawnedObjects[currentIndex].gameObject);
        }

        spawnedObjects[currentIndex] = Instantiate(spawnPrefab, (Vector2)transform.position + spawnPosition, Quaternion.identity, transform);
    }
}
