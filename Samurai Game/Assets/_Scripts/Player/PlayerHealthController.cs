using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// MOST OF THIS IS TEMPORARY TESTING RN

public class PlayerHealthController : MonoBehaviour
{
    [Header("HUD References:")]
    [SerializeField] private HealthBarController healthBarController;

    [Space(10)]

    [Header("Health Settings:")]
    [SerializeField] private int maxHealth;
    [ReadOnlyInspector] [SerializeField] private int currentHealth;

    [Space(10)]

    [Header("Respawn Settings:")]
    [SerializeField] private float respawnDelay;

    private PlayerCharacter playerCharacter;

    private InputAction takeDamageTEMPORARYAction;

    public event Action OnPlayerDeathEvent;
    public event Action OnPlayerRespawnEvent;

    public bool IsDead { get; private set;}

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

    private void Start()
    {
        if (!healthBarController)
        {
            healthBarController = FindFirstObjectByType<HealthBarController>();
        }

        healthBarController.SetMaxHealth(maxHealth);
    }

    private void Update()
    {
        if (IsDead)
        {
            playerCharacter.PlayerAnimationController.SetBool("canMove", false);
        }
    }
    #endregion UnityMethods

    #region HealthMethods
    private void OnTakeDamageTEMPORARYPressed(InputAction.CallbackContext context)
    {
        // For testing purposes, always to the right of the player (knocked back to the left)
        TakeDamage(1, new Vector2(transform.rotation.eulerAngles.y == 180.0f ? transform.position.x - 1.0f : transform.position.x + 1.0f, transform.position.y));
    }

    public void TakeDamage(int damage, Vector2 instigatorPosition)
    {
        if (IsDead) { return; }

        currentHealth -= damage;
        healthBarController.SetHealth(currentHealth);
        playerCharacter.PlayerAnimationController.SetTrigger("isTakingHit");
        playerCharacter.PlayerMovementController.SetKnockbackDirection(instigatorPosition);

        if (currentHealth <= 0)
        {
            IsDead = true;
            playerCharacter.PlayerAnimationController.SetBool("isDead", true);
            OnPlayerDeathEvent?.Invoke();
        }

    }

    public IEnumerator RespawnCoroutine()
    {
        AudioManager.Instance.PlaySFX("PlayerRespawn", 0.75f);

        yield return new WaitForSeconds(respawnDelay);

        IsDead = false;
        currentHealth = maxHealth;
        healthBarController.SetHealth(maxHealth);
        transform.position = new Vector2(0, 0);
        playerCharacter.PlayerAnimationController.ResetTrigger("isAttacking");
        playerCharacter.PlayerAnimationController.SetBool("isDead", false);
        playerCharacter.PlayerAnimationController.SetBool("canMove", true);
        OnPlayerRespawnEvent?.Invoke();
    }
    #endregion HealthMethods
}
