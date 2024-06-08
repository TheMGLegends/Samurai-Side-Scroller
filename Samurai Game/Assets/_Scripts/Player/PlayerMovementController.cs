using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

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
    [SerializeField] private float jumpForce;
    [Range(0, 1)] [SerializeField] private float jumpCutMultiplier;
    [SerializeField] private float gravityScale;
    [SerializeField] private float fallGravityMultiplier;
    [SerializeField] private float jumpCoyoteTime;
    [SerializeField] private float jumpBufferTime;
    [SerializeField] private Vector2 boxDimensions;
    [SerializeField] private LayerMask groundLayerMask;

    private PlayerCharacter playerCharacter;

    private InputAction movementAction;
    private InputAction jumpAction;

    private Rigidbody2D rb2D;

    // INFO: Movement Variables
    private float movementDirection;

    // INFO: Jump Variables
    private float lastGroundedTime;
    private float lastJumpTime;
    private bool canJump;
    private bool jumpInputReleased;

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

        jumpAction = playerCharacter.PlayerInputActions.Player.Jump;
        jumpAction.Enable();
        jumpAction.started += OnJumpPressed;
        jumpAction.canceled += OnJumpReleased;

    }

    private void OnDisable()
    {
        movementAction.Disable();
        movementAction.performed -= OnMovement;

        jumpAction.Disable();
        jumpAction.started -= OnJumpPressed;
        jumpAction.canceled -= OnJumpReleased;
    }

    private void Update()
    {
        IsGrounded();

        // INFO: Coyote Timers
        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        Move();
        Jump();
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
        if (lastGroundedTime > 0.0f && Mathf.Abs(movementDirection) < 0.01f)
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
    private void OnJumpPressed(InputAction.CallbackContext context)
    {
        jumpInputReleased = false;

        lastJumpTime = jumpBufferTime;
    }

    private void OnJumpReleased(InputAction.CallbackContext context)
    {
        jumpInputReleased = true;
    }

    private void IsGrounded()
    {
        Vector2 boxSize = new(Mathf.Abs(transform.localScale.x / 1.5f), boxDimensions.y);
        
        if (Physics2D.BoxCast(transform.position, boxSize, 0.0f, Vector2.down, boxDimensions.x, groundLayerMask))
        {
            canJump = true;
            lastGroundedTime = jumpCoyoteTime;
        }
    }

    private void Jump()
    {
        #region Jump
        if (lastGroundedTime > 0.0f && lastJumpTime > 0.0f && canJump)
        {
            canJump = false;

            lastGroundedTime = 0.0f;
            lastJumpTime = 0.0f;

            rb2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        #endregion Jump

        #region Fall
        if (rb2D.velocity.y > 0.0f && jumpInputReleased)
        {
            rb2D.AddForce((1 - jumpCutMultiplier) * rb2D.velocity.y * Vector2.down, ForceMode2D.Impulse);
        }
        #endregion Fall

        #region GravityChange
        if (rb2D.velocity.y < 0.0f)
        {
            rb2D.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else
        {
            rb2D.gravityScale = gravityScale;
        }
        #endregion GravityChange
    }
    #endregion JumpMethods
}
