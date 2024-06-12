using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionController : MonoBehaviour
{
    private PlayerCharacter playerCharacter;

    private InputAction attackAction;

    private void OnEnable()
    {
        attackAction = playerCharacter.PlayerInputActions.Player.Attack;
        attackAction.Enable();
        attackAction.started += OnAttack;
    }

    private void OnDisable()
    {
        attackAction.Disable();
        attackAction.started -= OnAttack;
    }

    public void Init(PlayerCharacter _playerCharacter)
    {
        playerCharacter = _playerCharacter;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        playerCharacter.PlayerAnimationController.SetTrigger("isAttacking");
    }
}
