using UnityEngine;

public class HelicopterDescend : MonoBehaviour
{
    private Vector2 startPosition;
    [SerializeField] private Transform attachPoint;
    [SerializeField] private Rigidbody2D helicopterRigidbody;
    [SerializeField] private Transform descendPoint;
    [SerializeField] private Transform leavePoint;
    private bool boarded;
    private static bool descend;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        startPosition = transform.position;
    }

    public static void Descend()
    {
        descend = true;
    }

    public void Enter()
    {
        gameObject.MyLevelManager().Player.Movement.SetAttached(helicopterRigidbody);
        gameObject.MyLevelManager().Player.Movement.PlacePlayer(attachPoint.position);
        boarded = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!descend)
        {
            return;
        }
        if (boarded)
        {
            Vector2 direction = (Vector2)leavePoint.position - helicopterRigidbody.position;

            if (direction.magnitude > 0.5f)
            {
                helicopterRigidbody.linearVelocity = direction * Time.fixedDeltaTime;
            }
            else
            {
                helicopterRigidbody.linearVelocity *= 0.98f;
            }
        }
        else
        {
            Vector2 direction = (Vector2)descendPoint.position - helicopterRigidbody.position;

            if (direction.magnitude > 0.5f)
            {
                helicopterRigidbody.linearVelocity = direction * Time.fixedDeltaTime;
            }
            else
            {
                helicopterRigidbody.linearVelocity *= 0.98f;
            }
        }
    }
}
