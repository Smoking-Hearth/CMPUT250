using UnityEngine;

public class HelicopterDescend : MonoBehaviour
{
    private Vector2 startPosition;
    [SerializeField] private Transform attachPoint;
    [SerializeField] private Rigidbody2D helicopterRigidbody;
    [SerializeField] private Transform descendPoint;
    [SerializeField] private Vector2 leaveDirection;
    [SerializeField] private float speed;
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

    public void Win()
    {
        if (FinalBoss.onWin != null)
        {
            FinalBoss.onWin();
        }
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
            helicopterRigidbody.linearVelocity = leaveDirection * speed * Time.fixedDeltaTime;
        }
        else
        {
            Vector2 direction = (Vector2)descendPoint.position - helicopterRigidbody.position;

            if (direction.magnitude > 0.5f)
            {
                helicopterRigidbody.linearVelocity = direction * speed * Time.fixedDeltaTime;
            }
            else
            {
                helicopterRigidbody.linearVelocity *= 0.98f;
            }
        }
    }
}
