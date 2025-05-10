using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonAIPatrolState : State
{
    [Header("Idle Settings")]

    [Range(0, 10)]
    [SerializeField] private float minIdleTime = 1.0f;

    [Range(0, 10)]
    [SerializeField] private float maxIdleTime = 2.5f;

    private readonly float rangeDifference = 0.1f;
    private Coroutine idleDurationCoroutine;


    [Header("Patrol Settings")]

    [Min(0)]
    [SerializeField] private float patrolSpeed = 1.0f;

    [Min(0)]
    [SerializeField] private float patrolRange = 5.0f;

    [Min(0)]
    [Tooltip("The character will stop moving when it is within this range of the destination")]
    [SerializeField] private float destinationDifference = 0.1f;

    private bool canMove = true;
    private float destinationX;

    private void OnValidate()
    {
        if (minIdleTime + rangeDifference > maxIdleTime)
        {
            maxIdleTime = minIdleTime + rangeDifference;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(destinationX, transform.position.y, 0.0f), destinationDifference);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position - new Vector3(patrolRange, 0.0f, 0.0f), transform.position + new Vector3(patrolRange, 0.0f, 0.0f));
    }

    public override void Enter()
    {
        ChooseDestination();
        aiCharacter.PlayAnimation("Patrol");
    }

    public override void Run()
    {
        // TODO: Check if target in range and switch to chase state

        if (!canMove) { return; }

        aiCharacter.transform.position = Vector2.MoveTowards(aiCharacter.transform.position, 
                                                             new Vector2(destinationX, aiCharacter.transform.position.y), 
                                                             patrolSpeed * Time.deltaTime);

        // TODO: NEED LOGIC TO DETECT WHEN THERE IS A LEDGE/WALL AND HAVE THEM TURN AROUND

        if (HasReachedDestination())
        {
            idleDurationCoroutine = StartCoroutine(IdleDurationCoroutine(Random.Range(minIdleTime, maxIdleTime)));
        }
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
        canMove = false;
        aiCharacter.PlayAnimation("Idle");

        yield return new WaitForSeconds(idleDuration);

        ChooseDestination();
        aiCharacter.PlayAnimation("Patrol");
        canMove = true;
    }

    private void ChooseDestination()
    {
        destinationX = Random.Range(-patrolRange, patrolRange);

        if (destinationX > 0)
        {
            aiCharacter.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (destinationX < 0)
        {
            aiCharacter.transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        destinationX += aiCharacter.transform.position.x;
    }

    private bool HasReachedDestination()
    {
        return Mathf.Abs(aiCharacter.transform.position.x - destinationX) <= destinationDifference;
    }
}
