using UnityEngine;

public class FinalBossFloor : MonoBehaviour
{
    private GameObject storedDoorObject;
    [SerializeField] private Transform door;
    [SerializeField] private Animator doorAnimator;

    [SerializeField] private FinalBossArm arm;

    [SerializeField] private float activateAltitude;
    private bool isOpen;

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
        }
        return true;
    }
}
