using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerStates
{
    None = 0,

    Idle,
    Run,
    Jump,
    Fall,
    Attack1,
    Attack2,
    TakeHit,
    Death
}

public class PlayerAnimationController : MonoBehaviour
{
    private PlayerCharacter playerCharacter;

    private Animator animator;

    /*[ReadOnlyInspector] [SerializeField] */private PlayerStates currentState;

    private void Awake()
    {
        animator = playerCharacter.Animator;
    }

    public void Init(PlayerCharacter _playerCharacter)
    {
        playerCharacter = _playerCharacter;
    }

    #region AnimationMethods
    public void ChangeAnimationState(PlayerStates newState)
    {
        // INFO: Prevents the same animation from being played again from the start
        if (currentState == newState)
            return;

        animator.Play(newState.ToString());
        currentState = newState;
    }

    public PlayerStates GetCurrentAnimationState()
    {
        return currentState;
    }

    public void ResetCurrentAnimationState()
    {
        currentState = PlayerStates.None;
    }

    public void SetTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    public void ResetTrigger(string triggerName)
    {
        animator.ResetTrigger(triggerName);
    }

    public void SetBool(string boolName, bool boolState)
    {
       animator.SetBool(boolName, boolState);
    }

    public bool GetBool(string boolName)
    {
        return animator.GetBool(boolName);
    }
    #endregion AnimationMethods
}
