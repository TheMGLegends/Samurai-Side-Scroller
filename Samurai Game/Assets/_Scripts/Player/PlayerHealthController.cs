using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// MOST OF THIS IS TEMPORARY TESTING RN

public class PlayerHealthController : MonoBehaviour
{
    [Header("Health Settings:")]
    [SerializeField] private int maxHealth;
    [ReadOnlyInspector] [SerializeField] private int currentHealth;

    private PlayerCharacter playerCharacter;

    private InputAction takeDamageTEMPORARYAction;

    private Coroutine lockMovementCoroutine;

    public void Init(PlayerCharacter _playerCharacter)
    {
        playerCharacter = _playerCharacter;
    }

    #region UnityMethods
    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        takeDamageTEMPORARYAction = playerCharacter.PlayerInputActions.Player.TakeDamageTEMPORARY;
        takeDamageTEMPORARYAction.Enable();
        takeDamageTEMPORARYAction.started += OnTakeDamageTEMPORARYPressed;
    }

    private void OnDisable()
    {
        takeDamageTEMPORARYAction.Disable();
        takeDamageTEMPORARYAction.started -= OnTakeDamageTEMPORARYPressed;
    }
    #endregion UnityMethods

    #region HealthMethods
    private void OnTakeDamageTEMPORARYPressed(InputAction.CallbackContext context)
    {
        // For testing purposes, always to the right of the player (knocked back to the left)
        TakeDamage(1, new Vector2(transform.position.x + 1, transform.position.y));
    }

    public void TakeDamage(int damage, Vector2 instigatorPosition)
    {
        currentHealth -= damage;

        Debug.Log("Current Health: " + currentHealth);

        playerCharacter.PlayerAnimationController.SetTrigger("isTakingHit");

        if (currentHealth <= 0)
        {
            playerCharacter.PlayerAnimationController.SetTrigger("isDead");
            Debug.Log("The player has died!");

            Invoke(nameof(Respawn), 2.0f);
        }

    }

    private void Respawn()
    {
        currentHealth = maxHealth;
        transform.position = new Vector2(0, 1);
        playerCharacter.PlayerAnimationController.SetBool("canMove", true);
    }
    #endregion HealthMethods
}
