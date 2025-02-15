using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerShoot))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float jumpBufferSeconds;

    private Vector2 inputAxes;

    [SerializeField] private Animator playerAnimator;

    private float jumpBufferEndTime;  //The max time that a jump input will be checked

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

    private PlayerShoot shootBehavior;  //The script that handles player shooting
    private bool isShooting;
    private bool isSpecialShooting;
    private bool isInteracting;

    void Awake()
    {
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
                    player.Sounds.PlayMainShoot();
                }
            }
        }
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
