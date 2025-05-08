using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : State
{
    [Range(0, 10)]
    [SerializeField] private float minIdleTime = 1.0f;

    [Range(0, 10)]
    [SerializeField] private float maxIdleTime = 2.5f;

    private readonly float rangeDifference = 0.1f;
    private Coroutine idleDurationCoroutine;

    private void OnValidate()
    {
        if (minIdleTime + rangeDifference > maxIdleTime)
        {
            maxIdleTime = minIdleTime + rangeDifference;
        }
    }

    public override void Enter()
    {
        if (aiCharacter.Animator != null) { aiCharacter.Animator.Play("Idle"); }
        idleDurationCoroutine = StartCoroutine(IdleDurationCoroutine(Random.Range(minIdleTime, maxIdleTime)));
    }

    public override void Exit()
    {
        if (idleDurationCoroutine != null)
        {
            StopCoroutine(idleDurationCoroutine);
            idleDurationCoroutine = null;
        }
    }

    private IEnumerator IdleDurationCoroutine(float idleDuration)
    {
        yield return new WaitForSeconds(idleDuration);

        // TODO: Transition to patrol state
    }
}
