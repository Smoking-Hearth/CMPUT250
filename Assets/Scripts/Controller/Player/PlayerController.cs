using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerShoot))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float jumpBufferSeconds;   //The amount of time after an input that a jump will still be registered
    [SerializeField] private float coyoteTimeSeconds;   //The amount of time after walking off a ledge that the player can still jump
    [SerializeField] private float dropBufferSeconds;   //Buffer time for "drop from platform" input

    private float jumpBufferEndTime;  //Timer for jump buffer
    private float coyoteEndTime;      //Timer for coyote time
    private float dropBufferEndTime;  //Timer for drop platform buffer


    private Vector2 inputAxes;  //The directional movement inputs that the player is currently inputting

    [SerializeField] private Animator playerAnimator;

    private bool shootEnabled;
    private static PlayerControls controls;
    public static PlayerControls Controls   //Other scripts can get this to listen to player inputs
    {
        get
        {
            if (controls == null)
            {
                controls = new PlayerControls();
                controls.PlayerMovement.Enable();
                controls.Menu.Enable();
            }
            return controls;
        }
    }

    private PlayerShoot shootBehavior;  //The script that handles player shooting
    private bool isShooting;            //Is the player holding the primary attack button
    private bool isSpecialShooting;     //Is the player holding the special attack button
    private bool isInteracting;         //Is the player holding the interact button

    void Awake()
    {
        if (shootBehavior == null)
        {
            shootBehavior = GetComponent<PlayerShoot>();
        }
    }

    private void OnEnable()
    {
        //Subscribe to all player inputs
        Controls.PlayerMovement.InputAxes.performed += OnAxisInput;
        Controls.PlayerMovement.InputAxes.canceled += OnAxisInput;
        Controls.PlayerMovement.Jump.performed += OnJumpInput;
        Controls.PlayerMovement.Jump.canceled += OnCancelJumpInput;
        Controls.PlayerMovement.DropDown.performed += OnDrop;
        Controls.Hydropack.Attack.performed += OnStartAttack;
        Controls.Hydropack.Attack.canceled += OnCancelAttack;
        Controls.Hydropack.SpecialAttack.performed += OnStartSpecial;
        Controls.Hydropack.SpecialAttack.canceled += OnCancelSpecial;
        Controls.Hydropack.SwapSpecial.performed += OnCancelSpecial;
        Controls.PlayerMovement.Interact.performed += OnInteract;
        Controls.PlayerMovement.Interact.canceled += OnCancelInteract;
    }

    private void OnDisable()
    {
        //Unsubscribe to all player inputs
        Controls.PlayerMovement.InputAxes.performed -= OnAxisInput;
        Controls.PlayerMovement.InputAxes.canceled -= OnAxisInput;
        Controls.PlayerMovement.Jump.performed -= OnJumpInput;
        Controls.PlayerMovement.Jump.canceled -= OnCancelJumpInput;
        Controls.PlayerMovement.DropDown.performed -= OnDrop;
        Controls.Hydropack.Attack.performed -= OnStartAttack;
        Controls.Hydropack.Attack.canceled -= OnCancelAttack;
        Controls.Hydropack.SpecialAttack.performed -= OnStartSpecial;
        Controls.Hydropack.SpecialAttack.canceled -= OnCancelSpecial;
        Controls.Hydropack.SwapSpecial.performed -= OnCancelSpecial;
        Controls.PlayerMovement.Interact.performed -= OnInteract;
        Controls.PlayerMovement.Interact.canceled -= OnCancelInteract;
    }

    private void Update()
    {
        Player player = gameObject.MyLevelManager().Player;
        if (player.GroundState == GroundState.None)
        {
            coyoteEndTime -= Time.fixedDeltaTime;
        }
        else
        {
            coyoteEndTime = coyoteTimeSeconds;
        }

        //Jumps if the player has pressed jump within the jump buffer time
        if (jumpBufferEndTime > 0)
        {
            if (coyoteEndTime > 0)
            {
                jumpBufferEndTime = 0;
                player.Movement.Jump();
            }
            else
            {
                jumpBufferEndTime -= Time.deltaTime;
            }
        }
    }

    private void FixedUpdate()
    {
        Player player = gameObject.MyLevelManager().Player;
        if (gameObject.MyLevelManager().levelState != LevelState.Playing)
        {
            if (controls.PlayerMovement.enabled)
            {
                controls.PlayerMovement.Disable();
                controls.Hydropack.Disable();
            }
        }
        else if (!controls.PlayerMovement.enabled)
        {
            controls.PlayerMovement.Enable();
            if (shootEnabled)
            {
                controls.Hydropack.Enable();
            }
        }

        player.Movement.Gravity();
        player.Movement.Move(inputAxes, !(isShooting || isSpecialShooting));

        if (dropBufferEndTime > 0)
        {
            dropBufferEndTime -= Time.deltaTime;
            if (player.Movement.DropPlatform())
            {
                dropBufferEndTime = 0;
            }
        }

        if (isSpecialShooting)  //Holding special attack (has priority over primary)
        {
            shootBehavior.AimSprites();
            if (!shootBehavior.AimStream())     //Stops special attack if out of the relavant resource
            {
                isSpecialShooting = false;
                shootBehavior.SpecialShoot(false);
            }
        }
        else if (isShooting)    //Holding primary attack
        {
            shootBehavior.AimSprites();
            if (shootBehavior.ShootAvailable)   //Checks if primary shoot is off cooldown
            {
                Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                if (shootBehavior.Shoot(targetPosition))
                {
                    player.Sounds.PlayMainShoot();
                }
            }
        }

        //Can only interact if not shooting
        if (!isShooting && !isSpecialShooting && isInteracting)
        {
            InteractableSystem.Interact(true);
        }
    }

    public void EnableShooting()
    {
        controls.Hydropack.Enable();
        shootBehavior.EnableShooting();
        shootEnabled = true;
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
        Player player = gameObject.MyLevelManager().Player;
        if (!isSpecialShooting)
        {
            player.Movement.CancelJump();
        }
    }

    private void OnDrop(InputAction.CallbackContext context)
    {
        Player player = gameObject.MyLevelManager().Player;
        
        if (!player.Movement.DropPlatform())
        {
            dropBufferEndTime = dropBufferSeconds;
        }
    }

    private void OnStartAttack(InputAction.CallbackContext context) 
    {
        isShooting = true;
        InteractableSystem.StopInteract();
    }

    private void OnCancelAttack(InputAction.CallbackContext context)
    {
        isShooting = false;
        shootBehavior.ResetAimedSprites();
    }

    private void OnStartSpecial(InputAction.CallbackContext context)
    {
        if (!isSpecialShooting && shootBehavior.SpecialAvailable)
        {
            if (shootBehavior.SpecialShoot(true))
            {
                isSpecialShooting = true;
                InteractableSystem.StopInteract();
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
            shootBehavior.ResetAimedSprites();
        }
        playerAnimator.SetBool("SpecialAttacking", isSpecialShooting);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        isInteracting = true;
        InteractableSystem.Interact(false);
    }
    private void OnCancelInteract(InputAction.CallbackContext context)
    {
        isInteracting = false;
        InteractableSystem.StopInteract();
    }
}
