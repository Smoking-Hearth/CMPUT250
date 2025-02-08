using UnityEngine;

public class FiretruckController : MonoBehaviour
{
    private enum State
    {
        Inactive,
        Idle,
        Accelerating,
        Deccelerating,
        Moving
    }

    [SerializeField] private PlayerController player;
    [SerializeField] private State firetruckState;
    [SerializeField] private Bounds activeArea;
    private Vector3 activeAreaOffset;

    [SerializeField] private Rigidbody2D firetruckRigidbody;
    [SerializeField] private float acceleration;
    [SerializeField] private float topSpeed;
    [SerializeField] private float endX;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        activeAreaOffset = activeArea.center;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch(firetruckState)
        {
            case State.Inactive:
                break;
            case State.Idle:
                IdleState();
                break;
            case State.Accelerating:
                Accelerate();
                break;
            case State.Deccelerating:
                Deccelerate();
                break;
            case State.Moving:
                MovingState();
                break;
        }
    }

    public void Activate()
    {
        firetruckState = State.Idle;
    }

    public void Deactivate()
    {
        firetruckState = State.Inactive;
    }

    private void IdleState()
    {
        player.SetAttached(null);
        activeArea.center = transform.position + activeAreaOffset;
        if (activeArea.Contains(GameManager.PlayerPosition) && transform.position.x < endX)
        {
            firetruckState = State.Accelerating;
        }
    }

    private void Accelerate()
    {
        player.SetAttached(firetruckRigidbody);
        if (firetruckRigidbody.linearVelocityX < topSpeed * Time.fixedDeltaTime)
        {
            firetruckRigidbody.linearVelocityX += acceleration * Time.fixedDeltaTime;
        }
        else
        {
            firetruckRigidbody.linearVelocityX = topSpeed * Time.fixedDeltaTime;
            firetruckState = State.Moving;
        }
    }
    private void Deccelerate()
    {
        if (firetruckRigidbody.linearVelocityX > 0)
        {
            firetruckRigidbody.linearVelocityX -= acceleration * Time.fixedDeltaTime;
        }
        else
        {
            firetruckRigidbody.linearVelocityX = 0;
            firetruckState = State.Idle;
        }
    }
    private void MovingState()
    {
        activeArea.center = transform.position + activeAreaOffset;
        firetruckRigidbody.linearVelocityX = topSpeed * Time.fixedDeltaTime;
        if (!activeArea.Contains(GameManager.PlayerPosition) || transform.position.x >= endX)
        {
            firetruckState = State.Deccelerating;
        }
    }
}
