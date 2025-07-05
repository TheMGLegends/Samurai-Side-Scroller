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
    [SerializeField] private GameObject healEffectPrefab;

    [Space(10)]

    [Header("Respawn Settings:")]
    [SerializeField] private float respawnDelay;

    private PlayerCharacter playerCharacter;
    private ParticleSystem healEffect;

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

    private void Start()
    {
        if (!healthBarController)
        {
            healthBarController = FindFirstObjectByType<HealthBarController>();
        }

        healthBarController.SetMaxHealth(maxHealth);

        // INFO: Instantiate Heal Effect and attach to player
        GameObject healEffectObject = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
        healEffectObject.transform.SetParent(transform);
        healEffect = healEffectObject.GetComponent<ParticleSystem>();
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
    public bool IsMaxHealth()
    {
        return maxHealth == currentHealth;
    }

    public void Heal(int healAmount)
    {
        if (IsDead) { return; }

        currentHealth += healAmount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        healthBarController.SetHealth(currentHealth, true);
        healEffect.Play(true);
        AudioManager.Instance.PlaySFX("Heal", 0.1f);
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
