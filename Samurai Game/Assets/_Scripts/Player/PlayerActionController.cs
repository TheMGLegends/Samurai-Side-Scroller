using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionController : MonoBehaviour
{
    [Header("Debug Settings:")]
    [SerializeField] private bool isInteractionBoxCastVisible;

    [Space(10)]

    [Header("Interaction Settings:")]
    [SerializeField] private LayerMask interactionLayerMask;
    [SerializeField] private Vector2 interactionBoxDimensions;
    [SerializeField] private Vector2 interactionBoxOffsets;

    private PlayerCharacter playerCharacter;

    private InputAction attackAction;
    private InputAction interactAction;

    public event Action OnAttackEvent;

    private void OnDrawGizmos()
    {
        // INFO: Draws a boxcast to visualize the interaction area
        if (isInteractionBoxCastVisible)
        {
            Gizmos.color = Color.green;

            Vector3 boxSize = new(interactionBoxDimensions.x, interactionBoxDimensions.y, 0.0f);
            Vector3 offsetPosition = new(transform.position.x + interactionBoxOffsets.x, transform.position.y + interactionBoxOffsets.y, transform.position.z);
            Gizmos.DrawWireCube(offsetPosition, boxSize);
        }
    }

    private void OnEnable()
    {
        attackAction = playerCharacter.PlayerInputActions.Player.Attack;
        attackAction.Enable();
        attackAction.started += OnAttack;

        interactAction = playerCharacter.PlayerInputActions.Player.Interact;
        interactAction.Enable();
        interactAction.started += OnInteract;

    }

    private void OnDisable()
    {
        attackAction.Disable();
        attackAction.started -= OnAttack;

        interactAction.Disable();
        interactAction.started -= OnInteract;
    }

    public void Init(PlayerCharacter _playerCharacter)
    {
        playerCharacter = _playerCharacter;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (playerCharacter.PlayerHealthController.IsDead)
            return;

        playerCharacter.PlayerAnimationController.SetTrigger("isAttacking");
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (playerCharacter.PlayerHealthController.IsDead) { return; }

        Vector3 boxSize = new(interactionBoxDimensions.x, interactionBoxDimensions.y, 0.0f);
        Vector2 origin = new(transform.position.x + interactionBoxOffsets.x, transform.position.y + interactionBoxOffsets.y);

        ContactFilter2D contactFilter = new();
        contactFilter.SetLayerMask(interactionLayerMask);
        contactFilter.useTriggers = true;

        RaycastHit2D[] hitResults = new RaycastHit2D[10];

        Physics2D.BoxCast(origin, boxSize, 0.0f, Vector2.zero, contactFilter, hitResults);

        foreach (RaycastHit2D hit in hitResults)
        {
            if (hit.collider == null) { continue; }

            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(gameObject);
            }
        }
    }

    /// <summary>
    /// Called by the animation event at the start of each attack to trigger the attack event
    /// </summary>
    public void BroadcastAttack()
    {
        OnAttackEvent?.Invoke();
    }
}
