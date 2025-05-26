using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GroundChaseState : State
{
    [Header("Gizmo Toggles")]

    [SerializeField] private bool drawTargetLoseBox = true;
    [SerializeField] private bool drawAttackRangeBox = true;


    [Header("Chase Settings")]

    [Min(0)]
    [SerializeField] private float chaseSpeed = 1.25f;


    [Header("Lose Target Settings")]

    [Min(0)]
    [SerializeField] private Vector2 targetLoseBox = Vector2.zero;

    [Min(0)]
    [SerializeField] private float timeUntilTargetLost = 1.0f;


    [Header("Attack Settings")]

    [SerializeField] private Vector2 attackRangeBox = Vector2.zero;


    private Coroutine lostTargetCoroutine = null;
    private bool ledgeDetected = false;
    private bool targetReached = false;


    private void OnDrawGizmosSelected()
    {
        // INFO: Draw the Target Lose Box
        if (drawTargetLoseBox)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, new Vector3(targetLoseBox.x, targetLoseBox.y));
        }

        // INFO: Draw the Attack Range Box
        if (drawAttackRangeBox)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(attackRangeBox.x, attackRangeBox.y));
        }
    }

    public override void Enter()
    {
        aiCharacter.movingPitch = 1.1f;

        if (aiCharacter.LedgeDetected())
        {
            ledgeDetected = true;
            aiCharacter.PlayAnimation("Idle");
        }
        else
        {
            ledgeDetected = false;
            aiCharacter.PlayAnimation("Chase");
        }
    }

    public override void Run()
    {
        TargetWithinAttackRange();
        DetectTargetLost();

        Vector2 targetPosition = aiCharacter.Target.transform.position;
        targetPosition.y = aiCharacter.transform.position.y;

        if (!aiCharacter.LedgeDetected())
        {
            if (ledgeDetected)
            {
                aiCharacter.PlayAnimation("Chase");
                ledgeDetected = false;
            }

            aiCharacter.transform.position = Vector2.MoveTowards(aiCharacter.transform.position, targetPosition, chaseSpeed * Time.deltaTime);

            if (aiCharacter.transform.position.x == targetPosition.x && !targetReached)
            {
                targetReached = true;
                aiCharacter.PlayAnimation("Idle");
            }
            else if (aiCharacter.transform.position.x != targetPosition.x && targetReached)
            {
                targetReached = false;
                aiCharacter.PlayAnimation("Chase");
            }
        }
        else
        {
            if (!ledgeDetected)
            {
                aiCharacter.PlayAnimation("Idle");
                ledgeDetected = true;
            }
        }

        aiCharacter.FaceDirection(Mathf.Sign(targetPosition.x - aiCharacter.transform.position.x));
    }

    public override void Exit()
    {
        if (lostTargetCoroutine != null)
        {
            StopCoroutine(lostTargetCoroutine);
            lostTargetCoroutine = null;
        }

        ledgeDetected = false;
        targetReached = false;
    }

    private IEnumerator LoseTargetCoroutine()
    {
        yield return new WaitForSeconds(timeUntilTargetLost);
        aiCharacter.SwitchState<GroundPatrolState>();
    }

    private void DetectTargetLost()
    {
        bool hasLostTarget = Physics2D.OverlapBox(transform.position, targetLoseBox, 0, aiCharacter.TargetMask) == null;

        if (hasLostTarget && lostTargetCoroutine == null)
        {
            lostTargetCoroutine = StartCoroutine(LoseTargetCoroutine());
        }
        else if (!hasLostTarget && lostTargetCoroutine != null)
        {
            StopCoroutine(lostTargetCoroutine);
            lostTargetCoroutine = null;
        }
    }

    private void TargetWithinAttackRange()
    {
        if (Physics2D.OverlapBox(transform.position, attackRangeBox, 0, aiCharacter.TargetMask))
        {
            aiCharacter.SwitchState<AttackState>();
        }
    }
}
