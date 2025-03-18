using UnityEngine;

public class FinalBossFloor : MonoBehaviour
{
    private GameObject storedDoorObject;
    [SerializeField] private Transform door;
    [SerializeField] private Animator doorAnimator;

    [SerializeField] private SpriteRenderer glassRenderer;
    [SerializeField] private Sprite glassBrokenSprite;
    [SerializeField] private FinalBossArm arm;
    private bool glassBroken;

    [SerializeField] private float activateAltitude;
    private bool isOpen;

    [SerializeField] private Vector2 dronePosition;
    [SerializeField] private DroneDropItem dronePrefab;


    // Update is called once per frame
    void FixedUpdate()
    {
        if (storedDoorObject != null && !isOpen)
        {
            if (gameObject.MyLevelManager().Player.Position.y >= door.position.y + activateAltitude)
            {
                OpenDoor();
            }
        }
    }

    public bool LoadDoor(GameObject prefab)
    {
        if (door == null)
        {
            return false;
        }
        if (isOpen && storedDoorObject != null)
        {
            return false;
        }

        storedDoorObject = Instantiate(prefab, door.position, Quaternion.identity, null).gameObject;
        storedDoorObject.SetActive(false);
        return true;
    }

    public void OpenDoor()
    {
        if (door == null)
        {
            return;
        }

        storedDoorObject.SetActive(true);
        isOpen = true;
        door.gameObject.SetActive(true);
        doorAnimator.SetTrigger("Open");
    }

    public bool ActivateArm()
    {
        if (!arm.IsActivated)
        {
            arm.gameObject.SetActive(true);
            arm.SetActive(true);
            glassBroken = true;
            glassRenderer.sprite = glassBrokenSprite;
        }
        return true;
    }

    public bool SendDrone()
    {
        if (dronePrefab == null || !glassBroken || arm.IsActivated)
        {
            return false;
        }

        Instantiate(dronePrefab, (Vector2)transform.position + dronePosition, Quaternion.identity, transform);
        return true;
    }
}
