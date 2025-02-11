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
    [SerializeField] private LayerMask stairsLayer;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private float gravityAcceleration = 30;
    [SerializeField] float jumpBufferSeconds;

    private Vector2 addedVelocity;
    private Vector2 targetMovement;
    private Vector2 inputAxes;

    [SerializeField] private Animator playerAnimator;
    [SerializeField] private PlayerSounds sounds;

    private float jumpBufferEndTime;  //The max time that a jump input will be checked
    private bool isGrounded;
    private bool isJumping;

    private Rigidbody2D attached;

    private static PlayerControls controls;
    public static PlayerControls Controls
    {
        get
        {
            if (controls == null)
            {
                controls = new PlayerControls();
                controls.Enable();
            }
            return controls;
        }
    }

    public PlayerSounds Sounds
    {
        get
        {
            return sounds;
        }
    }

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

        if (shootBehavior == null)
        {
            shootBehavior = GetComponent<PlayerShoot>();
        }
    }

    private void OnEnable()
    {
        Controls.PlayerMovement.InputAxes.performed += OnAxisInput;
        Controls.PlayerMovement.InputAxes.canceled += OnAxisInput;
        Controls.PlayerMovement.Jump.performed += OnJumpInput;
        Controls.PlayerMovement.Jump.canceled += OnCancelJumpInput;
        Controls.PlayerMovement.Attack.performed += OnStartAttack;
        Controls.PlayerMovement.Attack.canceled += OnCancelAttack;
        Controls.PlayerMovement.SpecialAttack.performed += OnStartSpecial;
        Controls.PlayerMovement.SpecialAttack.canceled += OnCancelSpecial;
        Controls.PlayerMovement.SwapSpecial.performed += OnCancelSpecial;
        SpecialAttack.onPushback += PushPlayer;
        Controls.PlayerMovement.Interact.performed += OnInteract;
        Controls.PlayerMovement.Interact.canceled += OnCancelInteract;
    }

    private void OnDisable()
    {
        Controls.PlayerMovement.InputAxes.performed -= OnAxisInput;
        Controls.PlayerMovement.InputAxes.canceled -= OnAxisInput;
        Controls.PlayerMovement.Jump.performed -= OnJumpInput;
        Controls.PlayerMovement.Jump.canceled -= OnCancelJumpInput;
        Controls.PlayerMovement.Attack.performed -= OnStartAttack;
        Controls.PlayerMovement.Attack.canceled -= OnCancelAttack;
        Controls.PlayerMovement.SpecialAttack.performed -= OnStartSpecial;
        Controls.PlayerMovement.SpecialAttack.canceled -= OnCancelSpecial;
        Controls.PlayerMovement.SwapSpecial.performed -= OnCancelSpecial;
        SpecialAttack.onPushback -= PushPlayer;
        Controls.PlayerMovement.Interact.performed -= OnInteract;
        Controls.PlayerMovement.Interact.canceled -= OnCancelInteract;
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
    }

    private void FixedUpdate()
    {
        if (GameManager.levelState != LevelState.Playing)
        {
            addedVelocity = Vector2.zero;
            targetMovement = Vector2.zero;
            playerRigidbody.gravityScale = 1;
            return;
        }

        Gravity();
        Move();

        if (isSpecialShooting)
        {
            shootBehavior.AimSprites();
            if (!shootBehavior.AimStream())
            {
                isSpecialShooting = false;
                shootBehavior.SpecialShoot(false);
            }
        }
        else if (isShooting)
        {
            shootBehavior.AimSprites();
            if (shootBehavior.ShootAvailable)
            {
                Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                if (shootBehavior.Shoot(targetPosition))
                {
                    sounds.PlayMainShoot();
                }
            }
        }

        if (!isShooting && !isSpecialShooting && isInteracting)
        {
            InteractableManager.Interact(true);
        }

        //Checks if the player is currently on stairs to make sure they don't slide down
        if (Physics2D.Raycast((Vector2)transform.position + groundCheckOffset, Vector2.down, groundCheckRadius + 0.1f, stairsLayer))
        {
            playerRigidbody.gravityScale = 0;
        }
        else
        {
            playerRigidbody.gravityScale = 1; //The player catches onto ledges, but slowly falls down
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
    }

    //Should only be called once per frame
    private void Move()
    {
        if (inputAxes.x == 0)  //Deccelerates if the player does not give a horizontal input
        {
            sounds.ResetFootsteps();
            if (targetMovement.x != 0)
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
        }
        else
        {
            //Accelerates according to horizontal input
            if ((inputAxes.x > 0 && targetMovement.x < moveSpeed) || (inputAxes.x < 0 && targetMovement.x > -moveSpeed))
            {
                float acceleration = groundAcceleration;

                if (!isGrounded)
                {
                    acceleration *= 0.6f;
                }

                targetMovement.x += inputAxes.x * acceleration;
                playerAnimator.SetBool("IsWalking", true);
            }
            else
            {
                targetMovement.x = inputAxes.x * moveSpeed;
            }
            if (isGrounded && Mathf.Sign(inputAxes.x) == Mathf.Sign(targetMovement.x))
            {
                sounds.PlayGrassFootsteps();
            }
        }
        if (!isShooting && !isSpecialShooting && targetMovement.x != 0)
        {
            shootBehavior.ResetAimedSprites(targetMovement.x < 0);
        }

        playerAnimator.SetFloat("MoveSpeed", targetMovement.x / moveSpeed);

        Vector2 finalVelocity = targetMovement + addedVelocity;

        playerRigidbody.linearVelocity = Time.fixedDeltaTime * finalVelocity;

        if (attached != null)
        {
            playerRigidbody.linearVelocity += attached.linearVelocity;
        }

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

    public void SetAttached(Rigidbody2D attach)
    {
        attached = attach;
    }

    public void PushPlayer(Vector2 acceleration)
    {
        if (addedVelocity.y > 0 && !isJumping)
        {
            addedVelocity.y *= 0.5f;
        }

        addedVelocity += acceleration;

        addedVelocity.x = Mathf.Clamp(addedVelocity.x, -terminalVelocity.x, terminalVelocity.x);
        addedVelocity.y = Mathf.Clamp(addedVelocity.y, -terminalVelocity.y, terminalVelocity.y);
    }

    //Accelerates the player downward
    private void Gravity()
    {
        if (!isGrounded)
        {
            addedVelocity.y -= gravityAcceleration;
        }
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
        if (GameManager.dialogSystem.DialogSystemState != DialogSystem.State.Inactive)
        {
            return;
        }
        isInteracting = true;
        InteractableManager.Interact(false);
    }
    private void OnCancelInteract(InputAction.CallbackContext context)
    {
        isInteracting = false;
        InteractableManager.StopInteract();
    }
}
