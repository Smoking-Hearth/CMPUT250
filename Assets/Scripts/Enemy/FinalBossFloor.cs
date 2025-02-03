using UnityEngine;

public class FinalBossFloor : MonoBehaviour
{
    private GameObject storedObject;
    [SerializeField] private Transform door;

    [SerializeField] private float activateAltitude;
    private bool isOpen;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (storedObject != null && !isOpen)
        {
            if (GameManager.PlayerPosition.y >= door.position.y + activateAltitude)
            {
                OpenDoor();
            }
        }
    }

    public bool LoadDoor(GameObject prefab)
    {
        if (isOpen && storedObject != null)
        {
            return false;
        }

        storedObject = Instantiate(prefab, door.position, Quaternion.identity, null).gameObject;
        storedObject.SetActive(false);
        return true;
    }

    public void OpenDoor()
    {
        storedObject.SetActive(true);
        isOpen = true;
    }
}
