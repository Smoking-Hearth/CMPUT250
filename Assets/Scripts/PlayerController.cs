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

    private Vector2 addedVelocity;
    private Vector2 targetMovement;
    private Vector2 inputAxes;
    
    private float jumpBufferEndTime;  //The max time that a jump input will be checked
    private bool isGrounded;
    private bool isJumping;
    public static PlayerControls controls;

    private PlayerShoot shootBehavior;  //The script that handles player shooting

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

        if (shootBehavior == null)
        {
            shootBehavior = GetComponent<PlayerShoot>();
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
        controls.PlayerMovement.Attack.performed -= OnAttack;
    }

    private void Update()
    {
        //Jumps if the player has pressed jump within the jump buffer time
        if (isGrounded && Time.time < jumpBufferEndTime)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        //Checks if the player is grounded
        if (Physics2D.OverlapCircle((Vector2)transform.position + groundCheckOffset, groundCheckRadius, groundLayer))
        {
            //Checks if the player has landed
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

    //Should only be called once per frame
    private void Move()
    {
        if (inputAxes.x == 0 && targetMovement.x != 0)  //Deccelerates if the player does not give a horizontal input
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
            //Accelerates according to horizontal input
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

    //Accelerates the player downward
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

    //INPUT HANDLING

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
        Vector2 targetPosition = Camera.main.ScreenToWorldPoint(ctx.ReadValue<Vector2>());
        shootBehavior.Shoot(targetPosition);
    }
}
