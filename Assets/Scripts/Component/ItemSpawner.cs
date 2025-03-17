using UnityEngine;
using UnityEngine.UI;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private Transform itemPrefab;
    [SerializeField] private Vector2 spawnPosition;
    [SerializeField] private float spawnInterval;
    [SerializeField] private Slider progressBar;
    private float spawnTimer;
    private Transform heldItem;

    void FixedUpdate()
    {
        if (heldItem != null)
        {
            if (!heldItem.gameObject.activeSelf || Vector2.Distance(heldItem.transform.position, transform.position) > 2)
            {
                heldItem = null;
            }
            return;
        }

        if (spawnTimer >= 0)
        {
            spawnTimer -= Time.fixedDeltaTime;

            if (progressBar != null)
            {
                progressBar.value = 1 - spawnTimer / spawnInterval;
            }

            if (spawnTimer < 0)
            {
                Transform spawnedItem = Instantiate(itemPrefab, (Vector2)transform.position + spawnPosition, Quaternion.identity, transform);
                heldItem = spawnedItem;
                spawnTimer = spawnInterval;
            }
        }
    }
}
