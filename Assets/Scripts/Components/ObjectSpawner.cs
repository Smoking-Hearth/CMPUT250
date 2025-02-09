using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPrefab;
    [Range(1, 20)]
    [SerializeField] private int maxObjects;
    [SerializeField] private Vector2 spawnPosition;
    private Transform[] spawnedObjects;
    private int currentIndex;
    private Transform lastDisabledObject;

    void Start()
    {
        spawnedObjects = new Transform[maxObjects];
    }

    public void Spawn()
    {
        currentIndex = (currentIndex + 1) % maxObjects;
        if (spawnedObjects[currentIndex] != null)
        {
            spawnedObjects[currentIndex].parent = null;
            spawnedObjects[currentIndex].gameObject.SetActive(false);

            if (lastDisabledObject != null)
            {
                Destroy(lastDisabledObject.gameObject);
            }
            lastDisabledObject = spawnedObjects[currentIndex];
        }

        spawnedObjects[currentIndex] = Instantiate(spawnPrefab, (Vector2)transform.position + spawnPosition, Quaternion.identity, transform);
    }
}
