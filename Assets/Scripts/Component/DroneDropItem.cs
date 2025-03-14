using UnityEngine;
using UnityEngine.Events;

public class DroneDropItem : MonoBehaviour
{
    [SerializeField] private float dropDistance;
    [SerializeField] private float speed;
    [SerializeField] private Transform droppedItem;
    [SerializeField] private Transform target;
    [SerializeField] private float targetHeight;
    [SerializeField] private UnityEvent droppedEvent;
    [SerializeField] private bool activated;
    private bool dropped;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Activate()
    {
        if (target == null)
        {
            target = gameObject.MyLevelManager().Player.Movement.transform;
        }
        activated = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!activated || dropped)
        {
            return;
        }
        MoveToTarget();
    }

    private void MoveToTarget()
    {
        Vector2 targetDirection = target.position - transform.position;
        Vector2 targetPosition = (Vector2)target.position - targetDirection.normalized * dropDistance + Vector2.up * targetHeight;
        Vector2 moveDirection = targetPosition - (Vector2)transform.position;

        if (moveDirection.magnitude > 0.2f)
        {
            transform.position = (Vector2)transform.position + moveDirection.normalized * speed * Time.fixedDeltaTime;
        }
        else
        {
            dropped = true;
            droppedItem.parent = transform.parent;
            droppedEvent.Invoke();
        }
    }
}
