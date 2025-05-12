using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : State
{
    [Header("Idle Settings")]

    [Range(0, 10)]
    [SerializeField] private float minIdleTime = 1.0f;

    [Range(0, 10)]
    [SerializeField] private float maxIdleTime = 2.5f;


    [Header("Patrol Settings")]

    [Min(0)]
    [SerializeField] private float patrolSpeed = 1.0f;

    [Min(0)]
    [Tooltip("The min half distance the character will patrol left and right from it's origin")]
    [SerializeField] private float minPatrolRange = 0.75f;

    [Min(0)]
    [Tooltip("The max half distance the character will patrol left and right from it's origin")]
    [SerializeField] private float maxPatrolRange = 2.5f;

    [Min(0)]
    [Tooltip("The character will stop moving when it is within this range of the destination")]
    [SerializeField] private float destinationDifference = 0.1f;

    [Tooltip("The layers that the character considers as walls")]
    [SerializeField] private LayerMask boundaryMask;


    private Coroutine idleDurationCoroutine;
    private readonly float rangeDifference = 0.1f;

    private bool canMove = true;
    private float destinationX;


    private void OnValidate()
    {
        if (minIdleTime + rangeDifference > maxIdleTime)
        {
            maxIdleTime = minIdleTime + rangeDifference;
        }

        if (minPatrolRange + rangeDifference > maxPatrolRange)
        {
            maxPatrolRange = minPatrolRange + rangeDifference;
        }
    }

    private void OnDrawGizmos()
    {
        if (UsePathfinding) { return; }

        // INFO: Draw the Patrol Destination Point
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(destinationX, transform.position.y, 0.0f), destinationDifference);
    }

    private void OnDrawGizmosSelected()
    {
        if (UsePathfinding) { return; }

        // INFO: Draw the Patrol Range
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position - new Vector3(maxPatrolRange, 0.0f, 0.0f), 
                        transform.position + new Vector3(maxPatrolRange, 0.0f, 0.0f));
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

        // INFO: Logic for Non-Pathfinding Characters
        if (!UsePathfinding)
        {
            aiCharacter.transform.position = Vector2.MoveTowards(aiCharacter.transform.position,
                                                                 new Vector2(destinationX, aiCharacter.transform.position.y),
                                                                 patrolSpeed * Time.deltaTime);

            // TODO: NEED LOGIC TO DETECT WHEN THERE IS A LEDGE AND HAVE THEM TURN AROUND

            if (HasReachedDestination())
            {
                idleDurationCoroutine = StartCoroutine(IdleDurationCoroutine(Random.Range(minIdleTime, maxIdleTime)));
            }
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
        if (!UsePathfinding)
        {
            Vector2 aiPosition = aiCharacter.transform.position;
            Vector2 colliderSize = aiCharacter.GetColliderSize();

            float distance = Random.Range(minPatrolRange, maxPatrolRange);
            float movingDirection = Mathf.Sign(Random.Range(0, 2) * 2 - 1); // INFO: -1 or 1

            aiCharacter.FaceMovingDirection(movingDirection);

            // INFO: Adjust the destination to be relative to the character's position
            destinationX = aiPosition.x + (distance * movingDirection);

            // INFO: Check for Walls and Adjust the Destination
            RaycastHit2D hit = Physics2D.Raycast(aiPosition, Vector2.right * movingDirection, distance + colliderSize.x, boundaryMask);

            if (hit.collider != null)
            {
                // INFO: If there is a wall, set the destination to the wall position less the character's size
                destinationX = hit.point.x - (colliderSize.x * movingDirection);

                // INFO: If the character is too close to the wall, invert the direction
                if (Mathf.Abs(destinationX - aiPosition.x) <= colliderSize.x)
                {
                    movingDirection *= -1;
                    aiCharacter.FaceMovingDirection(movingDirection);
                    destinationX = aiPosition.x + (distance * movingDirection);
                }
            }
        }
    }

    private bool HasReachedDestination()
    {
        return Mathf.Abs(aiCharacter.transform.position.x - destinationX) <= destinationDifference;
    }
}
