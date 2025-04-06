using UnityEngine;

public class StaircaseSection : MonoBehaviour
{
    [SerializeField] private SpriteRenderer glassRenderer;
    [SerializeField] private Sprite glassBrokenSprite;
    [SerializeField] private FinalBossArm arm;
    private bool glassBroken;

    [SerializeField] private float activateAltitude;

    [SerializeField] private Vector2 dronePosition;
    [SerializeField] private DroneDropItem dronePrefab;


    // Update is called once per frame
    void FixedUpdate()
    {
    }

    public bool ActivateArm()
    {
        if (arm == null)
        {
            return false;
        }
        if (!arm.IsActivated)
        {
            arm.gameObject.SetActive(true);
            arm.SetActive(true);

            if (glassRenderer != null)
            {
                glassBroken = true;
                glassRenderer.sprite = glassBrokenSprite;
            }
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
