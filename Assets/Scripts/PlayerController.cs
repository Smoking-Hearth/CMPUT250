using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float jumpPower;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundAcceleration;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private Vector2 groundCheckOffset;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float gravityAcceleration = 30;
    [SerializeField] float jumpBufferSeconds;

    [Header("Attack")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject bulletSpawnAt;
    [SerializeField] private float bulletInitialSpeed = 10.0f;

    private Vector2 addedVelocity;
    private Vector2 targetMovement;
    private Vector2 inputAxes;

    
    private float jumpBufferEndTime;
    private bool isGrounded;
    private bool isJumping;
    public static PlayerControls controls;

    public delegate void OnLand(Vector2 landPosition, float force);
    public static event OnLand onLand;

    public delegate void OnJump(Vector2 jumpPosition);
    public static event OnJump onJump;

    void Awake()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody2D>();
        }

        if (controls == null)
        {
            controls = new PlayerControls();
            controls.Enable();
        }
    }

    private void OnEnable()
    {
        controls.PlayerMovement.InputAxes.performed += OnAxisInput;
        controls.PlayerMovement.InputAxes.canceled += OnAxisInput;
        controls.PlayerMovement.Jump.performed += OnJumpInput;
        controls.PlayerMovement.Jump.canceled += OnCancelJumpInput;
        controls.PlayerMovement.Attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        controls.PlayerMovement.InputAxes.performed -= OnAxisInput;
        controls.PlayerMovement.InputAxes.canceled -= OnAxisInput;
        controls.PlayerMovement.Jump.performed -= OnJumpInput;
        controls.PlayerMovement.Jump.canceled -= OnCancelJumpInput;
    }

    private void Update()
    {
        if (isGrounded && Time.time < jumpBufferEndTime)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        if (Physics2D.OverlapCircle((Vector2)transform.position + groundCheckOffset, groundCheckRadius, groundLayer))
        {
            if (addedVelocity.y < 0)
            {
                isGrounded = true;
                isJumping = false;

                if (onLand != null)
                {
                    onLand(transform.position, -addedVelocity.y);
                }

                addedVelocity.y = 0;
            }
        }
        else
        {
            isGrounded = false;
        }
        Gravity();
        Move();
    }

    private void Move()
    {
        if (inputAxes.x == 0 && targetMovement.x != 0)
        {
            if (Mathf.Abs(targetMovement.x) > 0.02f)
            {
                targetMovement.x *= 0.9f;
            }
            else
            {
                targetMovement.x = 0;
            }
        }
        else
        {
            if ((inputAxes.x > 0 && targetMovement.x < moveSpeed) || (inputAxes.x < 0 && targetMovement.x > -moveSpeed))
            {
                targetMovement.x += inputAxes.x * groundAcceleration;
            }
            else
            {
                targetMovement.x = inputAxes.x * moveSpeed;
            }
        }

        playerRigidbody.linearVelocity = Time.fixedDeltaTime * (targetMovement + addedVelocity);
    }

    private void Gravity()
    {
        addedVelocity.y -= gravityAcceleration;
    }

    private void Jump()
    {
        if (!isGrounded || isJumping)
        {
            return;
        }

        isJumping = true;
        addedVelocity.y = jumpPower;

        if (onJump != null)
        {
            onJump(transform.position);
        }
    }

    private void OnAxisInput(InputAction.CallbackContext context)
    {
        inputAxes = context.ReadValue<Vector2>();
    }

    private void OnJumpInput(InputAction.CallbackContext context)
    {
        jumpBufferEndTime = Time.time + jumpBufferSeconds;
    }

    private void OnCancelJumpInput(InputAction.CallbackContext context)
    {
        if (addedVelocity.y > 0)
        {
            addedVelocity.y *= 0.5f;
        }
    }

    private void OnAttack(InputAction.CallbackContext ctx) 
    {
        GameObject firedBullet = Instantiate(bullet, bulletSpawnAt.transform.position, Quaternion.identity);

        var rigidBody = firedBullet.GetComponent<Rigidbody2D>();
        if (rigidBody != null) 
        {
            Vector2 dir = Camera.main.ScreenToWorldPoint(ctx.ReadValue<Vector2>()) - bulletSpawnAt.transform.position;
            dir.Normalize();
            rigidBody.linearVelocity = dir * bulletInitialSpeed;
        }
        else 
        {
            Debug.LogWarning("Water bullet prefab is missing a RigidBody. May be floating");
        }

        var timer = firedBullet.GetComponent<Timer>();
        if (timer != null)
        {
            timer.timeLeft_s = 2.0f;
            timer.OnFinishCallback += (gameObject) => {
                Destroy(gameObject);
            };
        }   
        else 
        {
            Debug.LogWarning("Water bullets will live forever. Performance problem.");
        }
    }
}
