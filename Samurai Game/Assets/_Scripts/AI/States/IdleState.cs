using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    public override void Enter()
    {
        if (aiCharacter.Animator != null)
        {
            aiCharacter.Animator.Play("Idle");
        }
    }

    public override void Exit()
    {
    }

    public override void Run()
    {
    }
}
