using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerMovementController : MonoBehaviour
{
    [Header("Debug Settings:")]
    [SerializeField] private bool isBoxCastVisible;

    [Header("Movement Settings:")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float velocityPower;
    [SerializeField] private float frictionAmount;

    [Space(10)]

    [Header("Jump Settings:")]
    [SerializeField] private Vector2 boxDimensions;
    [SerializeField] private LayerMask groundLayerMask;

    private PlayerCharacter playerCharacter;

    private InputAction movementAction;

    private Rigidbody2D rb2D;

    // INFO: Movement Variables
    private float movementDirection;

    #region UnityMethods
    private void OnDrawGizmos()
    {
        // INFO: Draws a boxcast to visualize the ground detection
        if (isBoxCastVisible)
        {
            Gizmos.color = Color.yellow;

            Vector3 boxSize = new(Mathf.Abs(transform.localScale.x / 1.5f), boxDimensions.y, 0.0f);
            Gizmos.DrawWireCube(transform.position + Vector3.down * boxDimensions.x, boxSize);
        }
    }

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        movementAction = playerCharacter.PlayerInputActions.Player.Movement;
        movementAction.Enable();
        movementAction.performed += OnMovement;
    }

    private void OnDisable()
    {
        movementAction.Disable();
        movementAction.performed -= OnMovement;
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        Move();
    }
    #endregion UnityMethods

    public void Init(PlayerCharacter _playerCharacter)
    {
        playerCharacter = _playerCharacter;
    }

    #region MovementMethods
    private void OnMovement(InputAction.CallbackContext context)
    {
        movementDirection = context.ReadValue<Vector2>().x;

        // INFO: Ensures movement direction is either -1 or 1 when using controller input
        if (movementDirection != 0) 
            movementDirection /= Mathf.Abs(movementDirection);

        // INFO: Flip player object based on movement direction
        transform.localScale = new Vector2(movementDirection != 0.0f ? movementDirection : transform.localScale.x, 1);
    }

    private void Move()
    {
        #region Run
        // INFO: Calculate the target speed based on the movement direction
        float targetSpeed = movementDirection * movementSpeed;

        // INFO: Calculate the speed difference between the target speed and the current velocity
        float speedDiff = targetSpeed - rb2D.velocity.x;

        // INFO: Calculate the acceleration rate based on the target speed
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;

        // INFO: Apply acceleration to speed difference, raising it to a power to make the movement feel more responsive
        // then multiply by sign of speed difference to maintain direction
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, velocityPower) * Mathf.Sign(speedDiff);

        rb2D.AddForce(Vector2.right * movement);
        #endregion Run

        #region Friction
        // INFO: Only apply friction when the player is grounded trying to stop
        if (IsGrounded() && Mathf.Abs(movementDirection) < 0.01f)
        {
            // INFO: Depending on what is smaller, that is the amount of friction to apply
            float amount = Mathf.Min(Mathf.Abs(rb2D.velocity.x), Mathf.Abs(frictionAmount));

            // INFO: Set Movement Direction
            amount *= Mathf.Sign(rb2D.velocity.x);

            // INFO: Apply force against movement direction
            rb2D.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
        #endregion Friction
    }
    #endregion MovementMethods

    #region JumpMethods
    private bool IsGrounded()
    {
        Vector2 boxSize = new(Mathf.Abs(transform.localScale.x / 1.5f), boxDimensions.y);
        return Physics2D.BoxCast(transform.position, boxSize, 0.0f, Vector2.down, boxDimensions.x, groundLayerMask);
    }
    #endregion JumpMethods
}
