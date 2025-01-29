using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerShoot))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float jumpPower;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundAcceleration;
    [SerializeField] private Vector2 terminalVelocity;
    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private Vector2 groundCheckOffset;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float gravityAcceleration = 30;
    [SerializeField] float jumpBufferSeconds;

    private Vector2 addedVelocity;
    private Vector2 targetMovement;
    private Vector2 inputAxes;

    [SerializeField] private Animator playerAnimator;

    private float jumpBufferEndTime;  //The max time that a jump input will be checked
    private bool isGrounded;
    private bool isJumping;
    public static PlayerControls controls;

    private PlayerShoot shootBehavior;  //The script that handles player shooting
    private bool isShooting;
    private bool isSpecialShooting;

    private bool isInteracting;

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
        controls.PlayerMovement.Attack.performed += OnStartAttack;
        controls.PlayerMovement.Attack.canceled += OnCancelAttack;
        controls.PlayerMovement.SpecialAttack.performed += OnStartSpecial;
        controls.PlayerMovement.SpecialAttack.canceled += OnCancelSpecial;
        HighPressureSpecial.onPushback += PushPlayer;
        controls.PlayerMovement.Interact.performed += OnInteract;
        controls.PlayerMovement.Interact.canceled += OnCancelInteract;
    }

    private void OnDisable()
    {
        controls.PlayerMovement.InputAxes.performed -= OnAxisInput;
        controls.PlayerMovement.InputAxes.canceled -= OnAxisInput;
        controls.PlayerMovement.Jump.performed -= OnJumpInput;
        controls.PlayerMovement.Jump.canceled -= OnCancelJumpInput;
        controls.PlayerMovement.Attack.performed -= OnStartAttack;
        controls.PlayerMovement.Attack.canceled -= OnCancelAttack;
        controls.PlayerMovement.SpecialAttack.performed -= OnStartSpecial;
        controls.PlayerMovement.SpecialAttack.canceled -= OnCancelSpecial;
        HighPressureSpecial.onPushback -= PushPlayer;
        controls.PlayerMovement.Interact.performed -= OnInteract;
        controls.PlayerMovement.Interact.canceled -= OnCancelInteract;
    }

    private void Update()
    {
        //Jumps if the player has pressed jump within the jump buffer time
        if (jumpBufferEndTime > 0)
        {
            if (isGrounded)
            {
                Jump();
            }
            else
            {
                jumpBufferEndTime -= Time.deltaTime;
            }
        }

        if (isSpecialShooting)
        {
            if (!shootBehavior.AimStream())
            {
                isSpecialShooting = false;
                shootBehavior.SpecialShoot(false);
            }
        }
        else if (isShooting && shootBehavior.ShootAvailable)
        {
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            shootBehavior.Shoot(targetPosition);
        }
    }

    private void FixedUpdate()
    {
        if (!isShooting && !isSpecialShooting && isInteracting)
        {
            InteractableManager.Interact(true);
        }
        //Checks if the player is grounded
        if (Physics2D.OverlapCircle((Vector2)transform.position + groundCheckOffset, groundCheckRadius, groundLayer))
        {
            //Checks if the player has landed
            if (addedVelocity.y < 0)
            {
                playerAnimator.SetBool("IsGrounded", true);
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
            playerAnimator.SetBool("IsGrounded", false);
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
            playerAnimator.SetBool("IsWalking", false);
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
                float acceleration = groundAcceleration * (isGrounded ? 1 : 0.6f);
                targetMovement.x += inputAxes.x * acceleration;
                playerAnimator.SetBool("IsWalking", true);
            }
            else
            {
                targetMovement.x = inputAxes.x * moveSpeed;
            }
        }

        playerAnimator.SetFloat("MoveSpeed", targetMovement.x / moveSpeed);

        Vector2 finalVelocity = targetMovement + addedVelocity;

        playerRigidbody.linearVelocity = Time.fixedDeltaTime * finalVelocity;

        if (addedVelocity.x != 0)
        {
            if (Mathf.Abs(addedVelocity.x) > 0.1f)
            {
                if (isGrounded)
                {
                    addedVelocity.x *= 0.8f;
                }
                else
                {
                    addedVelocity.x *= 0.99f;
                }
            }
            else
            {
                addedVelocity.x = 0;
            }
        }
    }

    private void PushPlayer(Vector2 acceleration)
    {
        if (addedVelocity.y > 0 && isJumping)
        {
            addedVelocity.y *= 0.5f;
            isJumping = false;
        }

        addedVelocity += acceleration;

        if (addedVelocity.x > terminalVelocity.x)
        {
            addedVelocity.x = terminalVelocity.x;
        }
        else if (addedVelocity.x < -terminalVelocity.x)
        {
            addedVelocity.x = -terminalVelocity.x;
        }

        if (addedVelocity.y > terminalVelocity.y)
        {
            addedVelocity.y = terminalVelocity.y;
        }
        else if (addedVelocity.y < -terminalVelocity.y)
        {
            addedVelocity.y = -terminalVelocity.y;
        }
    }

    //Accelerates the player downward
    private void Gravity()
    {
        addedVelocity.y -= gravityAcceleration;
    }

    private void Jump()
    {
        jumpBufferEndTime = 0;

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
        jumpBufferEndTime = jumpBufferSeconds;
    }

    private void OnCancelJumpInput(InputAction.CallbackContext context)
    {
        if (addedVelocity.y > 0 && isJumping && !isSpecialShooting)
        {
            addedVelocity.y *= 0.5f;
        }
    }

    private void OnStartAttack(InputAction.CallbackContext context) 
    {
        isShooting = true;
        InteractableManager.StopInteract();
    }

    private void OnCancelAttack(InputAction.CallbackContext context)
    {
        isShooting = false;
    }

    private void OnStartSpecial(InputAction.CallbackContext context)
    {
        if (!isSpecialShooting && shootBehavior.SpecialAvailable)
        {
            if (shootBehavior.SpecialShoot(true))
            {
                isSpecialShooting = true;
                InteractableManager.StopInteract();
                playerAnimator.SetBool("SpecialAttacking", isSpecialShooting);
            }
        }
    }

    private void OnCancelSpecial(InputAction.CallbackContext context)
    {
        if (isSpecialShooting)
        {
            isSpecialShooting = false;
            shootBehavior.SpecialShoot(false);
        }
        playerAnimator.SetBool("SpecialAttacking", isSpecialShooting);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        isInteracting = true;
        InteractableManager.Interact(false);
    }
    private void OnCancelInteract(InputAction.CallbackContext context)
    {
        isInteracting = false;
        InteractableManager.StopInteract();
    }
}
