using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeHitState : State
{
    [Header("Health Settings")]

    [Min(0)]
    [SerializeField] private int maxHealth = 1;

    [ReadOnlyInspector]
    [SerializeField] private int currentHealth = 0;

    [ReadOnlyInspector]
    [SerializeField] private int damageAmount = 0;


    private bool isTakingDamage = false;


    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public override void Enter()
    {
        // INFO: Have the hit animation restart if it is interrupted i.e (If the AI is hit again)
        if (currentHealth > 0)
        {
            aiCharacter.PlayAnimation("TakeHit", 0, 0.0f);
            isTakingDamage = true;
        }
    }

    public override void Run()
    {
        // INFO: Don't check animation state first time but do deal damage
        if (isTakingDamage)
        {
            currentHealth -= damageAmount;
            isTakingDamage = false;

            return;
        }

        if (!aiCharacter.AnimatorIsPlaying("TakeHit"))
        {
            if (currentHealth <= 0)
            {
                aiCharacter.SwitchState<DeathState>();
            }
            else
            {
                aiCharacter.SwitchState<GroundChaseState>();
            }
        }
    }

    public override void Exit()
    {
        isTakingDamage = false;
    }

    public void SetDamageAmount(int _damageAmount)
    {
        damageAmount = _damageAmount;
    }
}
