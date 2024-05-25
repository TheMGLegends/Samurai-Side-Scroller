using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

// CLEAN UP CODE <----------------------------------------------------------------------------------------------------------------------

public class PlayerMovementController : Subject
{
    [Header("Movement Settings:")]
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;

    private PlayerInputActions playerInputActions;
    private InputAction moveAction;
    private InputAction speedSwitchAction;

    private Rigidbody2D rb2D;

    private float movementDirection;
    public float currentSpeed;
    public bool isRunning;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        //currentSpeed = walkingSpeed;
    }

    private void OnEnable()
    {
        moveAction = playerInputActions.Player.Move;
        moveAction.Enable();

        speedSwitchAction = playerInputActions.Player.SpeedSwitch;
        speedSwitchAction.Enable();
        //speedSwitchAction.performed += OnSpeedSwitch;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        speedSwitchAction.Disable();
    }

    private void Update()
    {
        UpdateMovementDirection();
        UpdateCurrentSpeed();
    }

    private void FixedUpdate()
    {
        Move();
    }

    /*
    private void OnSpeedSwitch(InputAction.CallbackContext context)
    {
        // INFO: Everytime the speed switch action is performed, the current speed is toggled between walking and running speed
        if (context.performed)
        {
            currentSpeed = currentSpeed == walkingSpeed ? runningSpeed : walkingSpeed;
        }
    }
    */

    private void UpdateMovementDirection()
    {
        movementDirection = moveAction.ReadValue<Vector2>().x;

        // INFO: Flips the sprite based on the movement direction
        if (movementDirection != 0)
            transform.localScale = new Vector2(movementDirection, 1);
    }

    private void UpdateCurrentSpeed()
    {
        isRunning = speedSwitchAction.ReadValue<float>() > 0;

        if (isRunning)
            currentSpeed = runningSpeed;
        else
            currentSpeed = walkingSpeed;
    }

    private void Move()
    {
        rb2D.velocity = new Vector2(movementDirection * currentSpeed * Time.fixedDeltaTime, rb2D.velocity.y);

        if (rb2D.velocity.x == 0)
        {
            // INFO: Notifies observers that the player is idle
            NotifyObservers(EventType.PlayerAnimationStateChange, new EventData(PlayerAnimationStates.Idle.ToString()));
        }
        else
        {
            // INFO: Notifies observers that the player is moving
            NotifyObservers(EventType.PlayerAnimationStateChange, new EventData(PlayerAnimationStates.Run.ToString()));
        }
    }
}
