using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]

/// <summary>
/// Controls the Players' Actions (Movement, Jumping, Attacking, etc.)
/// </summary>
public class PlayerActionController : Subject
{
    [Header("Movement Settings:")]
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;

    private PlayerInputActions playerInputActions;
    private InputAction movementDirectionAction;
    private InputAction switchMoveSpeedAction;

    private Rigidbody2D rb2D;
    private float movementDirection;
    private float currentSpeed;

    #region UnityMethods
    private void OnEnable()
    {
        movementDirectionAction = playerInputActions.Player.Movement;
        movementDirectionAction.Enable();

        switchMoveSpeedAction = playerInputActions.Player.MovementSwitch;
        switchMoveSpeedAction.Enable();
    }

    private void OnDisable()
    {
        movementDirectionAction.Disable();
        switchMoveSpeedAction.Disable();
    }

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        rb2D = GetComponent<Rigidbody2D>();
    }


    private void Update()
    {
        UpdateMovementDirection();
        UpdateMovementSpeed();
    }

    private void FixedUpdate()
    {
        Move();
    }
    #endregion UnityMethods

    #region InputSystemReadingMethods
    private void UpdateMovementDirection()
    {
        movementDirection = movementDirectionAction.ReadValue<Vector2>().x;

        // INFO: Flips the sprite based on the movement direction
        if (movementDirection != 0)
            transform.localScale = new Vector2(movementDirection, 1);
    }

    private void UpdateMovementSpeed()
    {
        bool isRunning = switchMoveSpeedAction.ReadValue<float>() > 0;

        // INFO: Prevents re-assigment of the current speed if the player is already moving at the desired speed
        if (isRunning && currentSpeed != runningSpeed)
            currentSpeed = runningSpeed;
        else if (!isRunning && currentSpeed != walkingSpeed)
            currentSpeed = walkingSpeed;
    }
    #endregion InputSystemReadingMethods

    #region MovementMethods
    private void Move()
    {
        rb2D.velocity = new Vector2(movementDirection * currentSpeed * Time.fixedDeltaTime, rb2D.velocity.y);

        if (rb2D.velocity.x == 0)
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
    #endregion Movement
}
