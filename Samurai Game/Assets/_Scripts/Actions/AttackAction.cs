using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAction : MonoBehaviour
{
    [Header("Attack Settings")]

    [SerializeField] private int damageAmount = 1;
    [SerializeField] private LayerMask targetMask;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Helpers.IsInLayerMask(targetMask, collision.gameObject.layer)) { return; }

        // INFO: Check if the object is the Player
        if (collision.gameObject.TryGetComponent(out PlayerHealthController playerHealthController))
        {
            playerHealthController.TakeDamage(damageAmount, transform.position);
        }
        else if (collision.gameObject.TryGetComponent(out AICharacter aiCharacter))
        {
            Type currentState = aiCharacter.GetCurrentState();

            if (currentState != typeof(GroundDeathState) &&
                currentState != typeof(AirDeathState) &&
                currentState != typeof(ShieldState))
            {
                TakeHitState hitState = aiCharacter.SwitchState<TakeHitState>();
                hitState.SetDamageAmount(damageAmount);
            }
        }
    }
}
