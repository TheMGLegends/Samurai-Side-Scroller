using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldState : State
{
    [Header("Shield Settings")]

    [SerializeField] private Vector2 knockbackForce = new(0.75f, 0.0f);


    private bool hasShielded = true;


    public override void Enter()
    {
        hasShielded = true;
        AudioManager.Instance.PlaySFX("ShieldBlock", 0.25f);
        aiCharacter.PlayAnimation("Shield");
        aiCharacter.FaceDirection(Mathf.Sign(aiCharacter.Target.transform.position.x - aiCharacter.transform.position.x));
    }

    public override void Run()
    {
        // INFO: Prevent checking animation state first time
        if (hasShielded)
        {
            hasShielded = false;
            return;
        }

        if (!aiCharacter.AnimatorIsPlaying("Shield"))
        {
            aiCharacter.SwitchState<GroundChaseState>();
        }
        else
        {
            // INFO: Knockback Target
            if (aiCharacter.Target != null && aiCharacter.Target.TryGetComponent(out PlayerMovementController playerMovementController))
            {
                playerMovementController.KnockbackExternal(aiCharacter.transform.position, knockbackForce);
            }
        }
    }
}
