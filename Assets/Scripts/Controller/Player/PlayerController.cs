using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerShoot))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float jumpBufferSeconds;   //The amount of time after an input that a jump will still be registered
    [SerializeField] private float dropBufferSeconds;

    private Vector2 inputAxes;  //The directional movement inputs that the player is currently inputting

    [SerializeField] private Animator playerAnimator;

    private float jumpBufferEndTime;  //The max time that a jump input will be checked
    private float dropBufferEndTime;

    private static PlayerControls controls;
    public static PlayerControls Controls   //Other scripts can get this to listen to player inputs
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
        Controls.PlayerMovement.Attack.performed += OnStartAttack;
        Controls.PlayerMovement.Attack.canceled += OnCancelAttack;
        Controls.PlayerMovement.SpecialAttack.performed += OnStartSpecial;
        Controls.PlayerMovement.SpecialAttack.canceled += OnCancelSpecial;
        Controls.PlayerMovement.SwapSpecial.performed += OnCancelSpecial;
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
        Controls.PlayerMovement.Attack.performed -= OnStartAttack;
        Controls.PlayerMovement.Attack.canceled -= OnCancelAttack;
        Controls.PlayerMovement.SpecialAttack.performed -= OnStartSpecial;
        Controls.PlayerMovement.SpecialAttack.canceled -= OnCancelSpecial;
        Controls.PlayerMovement.SwapSpecial.performed -= OnCancelSpecial;
        Controls.PlayerMovement.Interact.performed -= OnInteract;
        Controls.PlayerMovement.Interact.canceled -= OnCancelInteract;
    }

    private void Update()
    {
        Player player = gameObject.MyLevelManager().Player;
        //Jumps if the player has pressed jump within the jump buffer time
        if (jumpBufferEndTime > 0)
        {
            if (player.GroundState != GroundState.None)
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
            return;
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
        if (gameObject.MyLevelManager().DialogSystem.DialogSystemState != DialogSystem.State.Inactive)
        {
            return;
        }
        isInteracting = true;
        InteractableSystem.Interact(false);
    }
    private void OnCancelInteract(InputAction.CallbackContext context)
    {
        isInteracting = false;
        InteractableSystem.StopInteract();
    }
}
