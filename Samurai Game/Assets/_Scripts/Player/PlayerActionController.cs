using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]

/// <summary>
/// Controls the Players' Actions (Movement, Jumping, Attacking, etc.)
/// </summary>
public class PlayerActionController : Subject
{
    [Header("Debug Settings:")]
    [SerializeField] private bool isBoxCastVisible;

    [Space(10)]

    [Header("Movement Settings:")]
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;

    [Space(10)]

    [Header("Jump Settings:")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpingGravityScale;
    [SerializeField] private float fallingGravityScale;
    [SerializeField] private float boxcastLength;
    [SerializeField] private LayerMask groundLayerMask;

    [Space(10)]

    [Header("Attack Settings:")]
    [SerializeField] private float attackDamage;

    private PlayerInputActions playerInputActions;
    private InputAction movementDirectionAction;
    private InputAction switchMoveSpeedAction;
    private InputAction jumpAction;
    private InputAction attackAction;

    private Rigidbody2D rb2D;
    private float movementDirection;
    private float currentSpeed;

    private bool isJumping;
    private bool hasJumped;

    private bool isAttacking;
    private bool switchAttack;
    private float attackDuration;
    private Animator animator;

    #region UnityMethods
    private void OnDrawGizmos()
    {
        // INFO: Draws a boxcast to visualize the ground detection
        if (isBoxCastVisible)
        {
            Gizmos.color = Color.yellow;

            Vector3 boxSize = new(Mathf.Abs(transform.localScale.x / 1.5f), 0.1f, 0.0f);
            Gizmos.DrawWireCube(transform.position + Vector3.down * boxcastLength, boxSize);
        }
    }

    private void OnEnable()
    {
        movementDirectionAction = playerInputActions.Player.Movement;
        movementDirectionAction.Enable();

        switchMoveSpeedAction = playerInputActions.Player.MovementSwitch;
        switchMoveSpeedAction.Enable();

        jumpAction = playerInputActions.Player.Jump;
        jumpAction.Enable();

        attackAction = playerInputActions.Player.Attack;
        attackAction.Enable();
        attackAction.performed += OnAttack;
    }

    private void OnDisable()
    {
        movementDirectionAction.Disable();

        switchMoveSpeedAction.Disable();

        jumpAction.Disable();

        attackAction.Disable();
        attackAction.performed -= OnAttack;
    }

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateMovementDirection();
        UpdateMovementSpeed();
        UpdateJump();
    }

    private void FixedUpdate()
    {
        Jump();
        Move();
    }
    #endregion UnityMethods

    #region InputSystemReadingMethods
    private void UpdateMovementDirection()
    {
        movementDirection = movementDirectionAction.ReadValue<Vector2>().x;

        if (movementDirection < 0.0f)
        {
            movementDirection = -1.0f;
        }
        else if (movementDirection > 0.0f)
        {
            movementDirection = 1.0f;
        }

        // INFO: Flips the sprite based on the movement direction
        if (movementDirection != 0)
        {
            transform.localScale = new Vector2(movementDirection, 1);
        }
    }

    private void UpdateMovementSpeed()
    {
        bool isRunning = switchMoveSpeedAction.ReadValue<float>() > 0;

        // INFO: Prevents re-assigment of the current speed if the player is already moving at the desired speed
        if (isRunning && currentSpeed != runningSpeed)
        {
            currentSpeed = runningSpeed;
        }
        else if (!isRunning && currentSpeed != walkingSpeed)
        {
            currentSpeed = walkingSpeed;
        }
    }

    private void UpdateJump()
    {
        if (jumpAction.WasPressedThisFrame())
        {
            isJumping = true;
            hasJumped = false;
        }
        
        if (jumpAction.WasReleasedThisFrame())
        {
            isJumping = false;
        }
    }
    #endregion InputSystemReadingMethods

    #region MovementMethods
    private void Move()
    {
        if (!isAttacking)
        {
            rb2D.velocity = new Vector2(movementDirection * currentSpeed * Time.fixedDeltaTime, rb2D.velocity.y);

            if (IsGrounded() && !isJumping)
            {
                // INFO: Resets the gravity scale to the default value ready for the jump
                rb2D.gravityScale = jumpingGravityScale;

                if (movementDirection == 0)
                {
                    // INFO: Notifies observers that the player is idle
                    NotifyObservers(EventType.AnimationStateChange, new EventData(PlayerAnimationStates.Idle.ToString()));
                }
                else
                {
                    // INFO: Notifies observers that the player is moving
                    NotifyObservers(EventType.AnimationStateChange, new EventData(PlayerAnimationStates.Run.ToString()));
                }
            }
        }
    }
    #endregion Movement

    #region JumpingMethods
    private bool IsGrounded()
    {
        Vector2 boxSize = new(Mathf.Abs(transform.localScale.x / 1.5f), 0.1f);
        return Physics2D.BoxCast(transform.position, boxSize, 0.0f, Vector2.down, boxcastLength, groundLayerMask);
    }

    private void Jump()
    {
        if (!isAttacking)
        {
            // INFO: Check for whether the jump action can be performed
            if (isJumping && IsGrounded())
            {
                // INFO: Ensures force only gets added once
                if (!hasJumped)
                {
                    rb2D.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);

                    hasJumped = true;
                }

                // INFO: Notifies observers that the player is jumping
                NotifyObservers(EventType.AnimationStateChange, new EventData(PlayerAnimationStates.Jump.ToString()));
            }

            // INFO: Variable jump height when the jump button is released
            if (!isJumping && rb2D.velocity.y > 0.0f)
            {
                rb2D.velocity = new Vector2(rb2D.velocity.x, 0.0f);
            }

            // INFO: Player falling check
            if (rb2D.velocity.y < 0.0f && !IsGrounded())
            {
                isJumping = false;

                // INFO: Increases the gravity scale to make the player fall faster
                rb2D.gravityScale = fallingGravityScale;

                // INFO: Notifies observers that the player is falling
                NotifyObservers(EventType.AnimationStateChange, new EventData(PlayerAnimationStates.Fall.ToString()));
            }
        }
    }
    #endregion JumpingMethods

    #region AttackingMethods
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (IsGrounded() && !isAttacking)
        {
            isAttacking = true;

            rb2D.velocity = Vector2.zero;

            if (switchAttack)
            {
                NotifyObservers(EventType.AnimationStateChange, new EventData(PlayerAnimationStates.Attack1.ToString()));
            }
            else
            {
                NotifyObservers(EventType.AnimationStateChange, new EventData(PlayerAnimationStates.Attack2.ToString()));
            }

            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        yield return null;

        Debug.Log(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name);
        attackDuration = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;

        yield return new WaitForSeconds(attackDuration);
        isAttacking = false;
        switchAttack = !switchAttack;
    }
    #endregion AttackingMethods
}
