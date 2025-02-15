using UnityEngine;

public class PlayerMovement : MonoBehaviour
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

    private Vector2 targetMovement;
    private Vector2 addedVelocity;

    [SerializeField] private Animator playerAnimator;

    private bool isJumping;

    private Rigidbody2D attached;

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
    }

    private void OnEnable()
    {
        SpecialAttack.onPushback += PushPlayer;
    }

    private void OnDisable()
    {
        SpecialAttack.onPushback -= PushPlayer;
    }

    private void FixedUpdate()
    {
        Player player = gameObject.MyLevelManager().Player;
        if (gameObject.MyLevelManager().levelState != LevelState.Playing)
        {
            addedVelocity = Vector2.zero;
            targetMovement = Vector2.zero;
            playerRigidbody.gravityScale = 1;
            return;
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
                player.GroundState = GroundState.Grass;
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
            player.GroundState = GroundState.None;
        }
    }

    //Should only be called once per frame
    public void Move(Vector2 inputAxes, bool flipGraphics)
    {
        Player player = gameObject.MyLevelManager().Player;
        if (inputAxes.x == 0)  //Deccelerates if the player does not give a horizontal input
        {
            player.Sounds.ResetFootsteps();
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

                if (player.GroundState == GroundState.None)
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
            if (player.GroundState != GroundState.None && Mathf.Sign(inputAxes.x) == Mathf.Sign(targetMovement.x))
            {
                player.Sounds.PlayGrassFootsteps();
            }
        }

        playerAnimator.SetFloat("MoveSpeed", targetMovement.x / moveSpeed);

        Vector2 finalVelocity = targetMovement + addedVelocity;

        if (flipGraphics && finalVelocity.x != 0)
        {
            playerAnimator.transform.localScale = new Vector2(Mathf.Sign(finalVelocity.x), 1);
        }

        playerRigidbody.linearVelocity = Time.fixedDeltaTime * finalVelocity;

        if (attached != null)
        {
            playerRigidbody.linearVelocity += attached.linearVelocity;
        }

        if (addedVelocity.x != 0)
        {
            if (Mathf.Abs(addedVelocity.x) > 0.1f)
            {
                if (player.GroundState != GroundState.None)
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
    public void Gravity()
    {
        Player player = gameObject.MyLevelManager().Player;
        if (player.GroundState == GroundState.None)
        {
            addedVelocity.y -= gravityAcceleration;
        }
    }

    public void Jump()
    {
        Player player = gameObject.MyLevelManager().Player;
        if (player.GroundState == GroundState.None || isJumping)
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

    public void CancelJump()
    {
        if (addedVelocity.y > 0 && isJumping)
        {
            addedVelocity.y *= 0.5f;
        }
    }
}
