using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDeathState : State
{
    [SerializeField] private Sprite deathSprite;

    private bool hasDied = true;

    public override void Enter()
    {
        aiCharacter.PlayAnimation("Death");
    }

    public override void Run()
    {
        // INFO: Prevent checking animation state first time
        if (hasDied)
        {
            hasDied = false;
            aiCharacter.InvokeDeathEvent();
            return;
        }

        if (!aiCharacter.AnimatorIsPlaying("Death"))
        {
            aiCharacter.Deactivate();
            aiCharacter.SpriteRenderer.sprite = deathSprite;
        }
    }
}
