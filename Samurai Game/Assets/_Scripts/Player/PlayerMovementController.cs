using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Debug Settings:")]
    [SerializeField] private bool isBoxCastVisible;

    [Space(10)]

    [Header("Follow Object:")]
    [SerializeField] private CameraFollowObject cameraFollowObject;

    [Space(10)]

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

    [Space(10)]

    [Header("Knockback Settings:")]
    [SerializeField] private Vector2 knockbackForce;

    [Space(10)]

    [Header("Dash Settings:")]
    [SerializeField] private float dashForce;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldownTime = 1.0f;
    [SerializeField] private float dashAfterImageIntervalDuration = 0.05f;
    [SerializeField] private GameObject dashAfterImagePrefab;
    [SerializeField] private int dashAfterImageCount = 10;

    private PlayerCharacter playerCharacter;

    private InputAction movementAction;
    private InputAction dashAction;
    private InputAction jumpAction;

    private Rigidbody2D rb2D;

    // INFO: Movement Variables
    private float movementDirection;
    private bool isFacingRight;
    private bool canMove;
    private float fallSpeedYDampingChangeThreshold;

    // INFO: Jump Variables
    private float lastGroundedTime;
    private float lastJumpTime;
    private bool isJumping;
    private bool jumpInputReleased;

    // INFO: Knockback Variables
    private Vector2 knockbackDirection;

    // INFO: Dash Variables
    private bool canDash = true;
    private bool isDashing;
    private List<SpriteAfterImage> dashAfterImagePool = new();

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
        rb2D = playerCharacter.Rb2D;
    }

    private void OnEnable()
    {
        movementAction = playerCharacter.PlayerInputActions.Player.Movement;

        // INFO: Rebind movement action bindings
        string moveLeftKeybind = "";
        string moveRightKeybind = "";

        if (PlayerPrefs.HasKey(ActionType.MoveLeft.ToString()))
        {
            moveLeftKeybind = PlayerPrefs.GetString(ActionType.MoveLeft.ToString());
        }

        if (PlayerPrefs.HasKey(ActionType.MoveRight.ToString()))
        {
            moveRightKeybind = PlayerPrefs.GetString(ActionType.MoveRight.ToString());
        }

        for (int i = 0; i < movementAction.bindings.Count; ++i)
        {
            var b = movementAction.bindings[i];
            if (!b.isPartOfComposite)
                continue;

            switch (b.name)
            {
                case "left":
                    if (string.IsNullOrWhiteSpace(moveLeftKeybind)) return;

                    movementAction.ChangeBinding(i)
                        .WithPath(moveLeftKeybind);
                    break;

                case "right":
                    if (string.IsNullOrWhiteSpace(moveRightKeybind)) return;

                    movementAction.ChangeBinding(i)
                        .WithPath(moveRightKeybind);
                    break;
            }
        }

        movementAction.Enable();
        movementAction.performed += OnMovement;

        dashAction = playerCharacter.PlayerInputActions.Player.Dash;

        if (PlayerPrefs.HasKey(ActionType.Dash.ToString()))
        {
            string dashKeybind = PlayerPrefs.GetString(ActionType.Dash.ToString());

            if (!string.IsNullOrWhiteSpace(dashKeybind))
            {
                dashAction.ChangeBinding(0).WithPath(dashKeybind);
            }
        }

        dashAction.Enable();
        dashAction.performed += OnDash;

        jumpAction = playerCharacter.PlayerInputActions.Player.Jump;

        if (PlayerPrefs.HasKey(ActionType.Jump.ToString()))
        {
            string jumpKeybind = PlayerPrefs.GetString(ActionType.Jump.ToString());

            if (!string.IsNullOrWhiteSpace(jumpKeybind))
            {
                jumpAction.ChangeBinding(0).WithPath(jumpKeybind);
            }
        }

        jumpAction.Enable();
        jumpAction.started += OnJumpPressed;
        jumpAction.canceled += OnJumpReleased;
    }

    private void OnDisable()
    {
        movementAction.Disable();
        movementAction.performed -= OnMovement;

        dashAction.Disable();
        dashAction.performed -= OnDash;

        jumpAction.Disable();
        jumpAction.started -= OnJumpPressed;
        jumpAction.canceled -= OnJumpReleased;
    }

    private void Start()
    {
        fallSpeedYDampingChangeThreshold = CameraManager.Instance.GetFallSpeedYDampingChangeThreshold;

        // INFO: Fill the after image pool
        if (dashAfterImagePrefab != null)
        {
            GameObject dashAfterImageContainer = new("DashAfterImagePool");

            for (int i = 0; i < dashAfterImageCount; ++i)
            {
                GameObject afterImage = Instantiate(dashAfterImagePrefab);

                if (afterImage && dashAfterImageContainer)
                {
                    afterImage.SetActive(false);
                    afterImage.transform.SetParent(dashAfterImageContainer.transform);
                    dashAfterImagePool.Add(afterImage.GetComponent<SpriteAfterImage>());
                }
            }
        }
    }

    private void Update()
    {
        IsGrounded();
        CanMove();
        Flip();

        // INFO: Coyote Timers
        lastGroundedTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;

        // INFO: If we are falling past a certain speed, change the camera damping
        if (rb2D.velocity.y < fallSpeedYDampingChangeThreshold && !CameraManager.Instance.IsSLerpingYDamping && !CameraManager.Instance.LerpedFromPlayerFalling)
        {
            CameraManager.Instance.LerpYDamping(true);
        }

        // INFO: If we are standing still or moving up, change the camera damping back to normal
        if (rb2D.velocity.y >= 0.0f && !CameraManager.Instance.IsSLerpingYDamping && CameraManager.Instance.LerpedFromPlayerFalling)
        {
            CameraManager.Instance.LerpedFromPlayerFalling = false;

            CameraManager.Instance.LerpYDamping(false);
        }
    }

    private void FixedUpdate()
    {
        Move();
        Jump();
        Dash();
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
    }

    private void Flip()
    {
        // INFO: If we can't move, return
        if (!canMove)
            return;

        // INFO: Flip the player if movement direction has changed, otherwise return
        if (movementDirection < 0.0f && !isFacingRight)
            isFacingRight = true;
        else if (movementDirection > 0.0f && isFacingRight)
            isFacingRight = false;
        else
            return;

        Vector3 rotator = new(transform.rotation.x, isFacingRight ? 180.0f : 0.0f, transform.rotation.z);
        transform.rotation = Quaternion.Euler(rotator);
        cameraFollowObject.Turn();
    }

    private void CanMove()
    {
        canMove = playerCharacter.PlayerAnimationController.GetBool("canMove");
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

        // INFO: Only move when we can
        if (!playerCharacter.PlayerAnimationController.GetBool("isKnockedback"))
        {
            if (canMove)
            {
                rb2D.AddForce(Vector2.right * movement);
            }
            else
            {
                rb2D.velocity = new Vector2(0.0f, rb2D.velocity.y);
            }
        }
        else
        {
            Knockback();
        }
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

        #region Animation
        if (lastGroundedTime > 0.0f && !isJumping && canMove)
        {
            playerCharacter.PlayerAnimationController.ChangeAnimationState(movementDirection != 0.0f ? PlayerStates.Run : PlayerStates.Idle);
        }
        #endregion Animation
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
            // INFO: Play Landing Sound
            if (lastGroundedTime < 0.0f)
            {
                AudioManager.Instance.PlaySFX("PlayerLand", 0.75f);
            }

            lastGroundedTime = jumpCoyoteTime;

            playerCharacter.PlayerAnimationController.SetBool("isGrounded", true);
        }
        else
        {
            playerCharacter.PlayerAnimationController.SetBool("isGrounded", false);
        }

    }

    private void Jump()
    {
        #region Jump
        if (lastGroundedTime > 0.0f && lastJumpTime > 0.0f && !isJumping && canMove)  
        {
            isJumping = true;

            lastGroundedTime = 0.0f;
            lastJumpTime = 0.0f;

            rb2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            playerCharacter.PlayerAnimationController.ChangeAnimationState(PlayerStates.Jump);
            AudioManager.Instance.PlaySFX("PlayerJump", 0.75f);
        }
        #endregion Jump

        #region Fall
        if (rb2D.velocity.y > 0.0f && (jumpInputReleased || playerCharacter.PlayerHealthController.IsDead))
        {
            rb2D.AddForce((1 - jumpCutMultiplier) * rb2D.velocity.y * Vector2.down, ForceMode2D.Impulse);
        }
        #endregion Fall

        #region GravityChange
        if (rb2D.velocity.y < 0.0f && lastGroundedTime < 0.0f)
        {
            isJumping = false;

            rb2D.gravityScale = gravityScale * fallGravityMultiplier;

            playerCharacter.PlayerAnimationController.ChangeAnimationState(PlayerStates.Fall);
        }
        else
        {
            rb2D.gravityScale = gravityScale;
        }
        #endregion GravityChange
    }
    #endregion JumpMethods

    #region DashMethods
    private void OnDash(InputAction.CallbackContext context)
    {
        // INFO: Return if we can't dash or if we can't move or if we are currently not moving
        if (!canDash || !canMove || movementDirection == 0.0f) { return; }

        // INFO: Shoot out a raycast in the travelling direction of the player, if we hit
        //       something prevent the player from dashing
        if (Physics2D.Raycast(transform.position, transform.right, 1.0f, groundLayerMask))
        {
            return;
        }

        isDashing = true;
        canDash = false;
        AudioManager.Instance.PlaySFX("PlayerDash", 1.5f, false, null, 1.0f, 500.0f, 0.5f);
        StartCoroutine(nameof(SpawnDashAfterImagesCoroutine));
        StartCoroutine(nameof(DashEndCoroutine));
    }

    private void Dash()
    {
        if (!isDashing) { return; }

        float dashDirection = isFacingRight ? -1.0f : 1.0f;
        rb2D.AddForce(dashForce * dashDirection * Vector2.right, ForceMode2D.Impulse);
    }

    private IEnumerator SpawnDashAfterImagesCoroutine()
    {
        // INFO: While we are dashing perform the following
        while (isDashing)
        {
            // INFO: Find an inactive dash after image
            foreach (SpriteAfterImage afterImage in dashAfterImagePool)
            {
                if (!afterImage.gameObject.activeInHierarchy)
                {
                    // INFO: Set position and rotation to match the game object
                    afterImage.transform.SetPositionAndRotation(transform.position, transform.rotation);
                    afterImage.transform.localScale = transform.localScale;

                    // INFO: Initialise the after image with the game objects own sprite renderer
                    afterImage.Initialise(playerCharacter.SpriteRenderer);

                    break;
                }
            }

            yield return new WaitForSeconds(dashAfterImageIntervalDuration);
        }
    }

    private IEnumerator DashEndCoroutine()
    {
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        StartCoroutine(nameof(DashCooledDownCoroutine));
    }

    private IEnumerator DashCooledDownCoroutine()
    {
        yield return new WaitForSeconds(dashCooldownTime);
        canDash = true;
    }
    #endregion DashMethods

    #region KnockbackMethods
    public void SetKnockbackDirection(Vector2 instigatorPosition)
    {
        // INFO: Compare instigator and player positions to determine knockback direction
        knockbackDirection = ((Vector2)transform.position - instigatorPosition).normalized;
    }

    private void Knockback()
    {
        //rb2D.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        rb2D.velocity = knockbackDirection * knockbackForce;
    }

    public void KnockbackExternal(Vector2 instigatorPosition, Vector2 force)
    {
        // INFO: Compare instigator and player positions to determine knockback direction
        SetKnockbackDirection(instigatorPosition);
        rb2D.AddForce(knockbackDirection * force, ForceMode2D.Impulse);
    }
    #endregion KnockbackMethods

}
