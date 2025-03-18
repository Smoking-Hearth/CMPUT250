using UnityEngine;

public class DisableCollider : MonoBehaviour
{
    [SerializeField] private bool disableWhenAbove;
    [SerializeField] private Collider2D disableCollider;
    [SerializeField] private float disableDistance;

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 playerPosition = gameObject.MyLevelManager().Player.Position;
        if (disableWhenAbove)
        {
            if (transform.position.y - disableDistance > playerPosition.y)
            {
                disableCollider.enabled = false;
            }
            else
            {
                disableCollider.enabled = true;
            }
        }
        else
        {
            if (transform.position.y + disableDistance < playerPosition.y)
            {
                disableCollider.enabled = false;
            }
            else
            {
                disableCollider.enabled = true;
            }
        }
    }
}
