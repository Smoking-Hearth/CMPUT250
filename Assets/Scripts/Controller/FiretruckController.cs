using UnityEngine;
using UnityEngine.Events;

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

    [SerializeField] private PlayerMovement player;
    [SerializeField] private State firetruckState;
    [SerializeField] private Bounds activeArea;
    private Vector3 activeAreaOffset;

    [SerializeField] private Rigidbody2D firetruckRigidbody;
    [SerializeField] private float acceleration;
    [SerializeField] private float topSpeed;
    [SerializeField] private float endX;

    private bool started;
    [SerializeField] private UnityEvent startEvent;
    [SerializeField] private UnityEvent completeEvent;
    [SerializeField] private UnityEvent stopEvent;
    [SerializeField] private UnityEvent continueEvent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        activeAreaOffset = activeArea.center;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!gameObject.ShouldUpdate()) return;

        if (gameObject.MyLevelManager().levelState == LevelState.Defeat)
        {
            return;
        }

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
        if (activeArea.Contains(gameObject.MyLevelManager().Player.Position) && transform.position.x < endX)
        {
            if (!started)
            {
                started = true;
                startEvent.Invoke();
            }
            else
            {
                continueEvent.Invoke();
            }
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
            firetruckRigidbody.linearVelocityX -= acceleration * Time.fixedDeltaTime * 2;
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
        if (transform.position.x >= endX)
        {
            completeEvent.Invoke();
            firetruckState = State.Deccelerating;
        }
        else if (!activeArea.Contains(gameObject.MyLevelManager().Player.Position))
        {
            stopEvent.Invoke();
            firetruckState = State.Deccelerating;
        }
    }
}
