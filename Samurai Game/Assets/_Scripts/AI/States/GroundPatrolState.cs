using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundPatrolState : State
{
    [Header("Gizmo Toggles")]

    [SerializeField] private bool drawPatrolDestinationPoint = true;
    [SerializeField] private bool drawPatrolRange = true;
    [SerializeField] private bool drawTargetDetectionBox = true;


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

    [Tooltip("The height at which the patrol range is drawn from the character origin")]
    [SerializeField] private float patrolLineVerticalOffset = -1.0f;


    [Header("Target Detection Settings")]

    [Min(0)]
    [Tooltip("The size of the box used to detect the target")]
    [SerializeField] private Vector2 targetDetectionBox = Vector2.zero;


    private Coroutine idleDurationCoroutine = null;
    private readonly float rangeDifference = 0.1f;
    private bool canMove = true;
    private float destinationX = 0.0f;


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

    private void OnDrawGizmosSelected()
    {
        // INFO: Draw the Patrol Range
        if (drawPatrolRange)
        {
            Gizmos.color = Color.green;
            Vector3 from = new(transform.position.x - maxPatrolRange, transform.position.y + patrolLineVerticalOffset, 0.0f);
            Vector3 to = new(transform.position.x + maxPatrolRange, transform.position.y + patrolLineVerticalOffset, 0.0f);
            Gizmos.DrawLine(from, to);
        }

        // INFO: Draw the Patrol Destination Point
        if (drawPatrolDestinationPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(destinationX, transform.position.y), destinationDifference);
        }

        // INFO: Draw the Target Detection Box
        if (drawTargetDetectionBox)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, targetDetectionBox);
        }
    }

    public override void Enter()
    {
        aiCharacter.movingPitch = 1.0f;

        ChooseDestination();
        aiCharacter.PlayAnimation("Patrol");
    }

    public override void Run()
    {
        DetectTarget();

        // INFO: Movement Logic
        if (canMove)
        {
            Vector2 targetPosition = new(destinationX, aiCharacter.transform.position.y);
            aiCharacter.transform.position = Vector2.MoveTowards(aiCharacter.transform.position, targetPosition, patrolSpeed * Time.deltaTime);

            if (aiCharacter.LedgeDetected())
            {
                Vector2 characterPosition = aiCharacter.transform.position;

                float distanceLeftToTravel = Mathf.Abs(destinationX - characterPosition.x);
                float movingDirection = -Mathf.Sign(destinationX - characterPosition.x);

                aiCharacter.FaceDirection(movingDirection);
                destinationX = characterPosition.x + (distanceLeftToTravel * movingDirection);
            }

            if (HasReachedDestination())
            {
                float idleDuration = Random.Range(minIdleTime, maxIdleTime);
                idleDurationCoroutine = StartCoroutine(IdleDurationCoroutine(idleDuration));
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

        canMove = true;
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
        Vector2 aiPosition = aiCharacter.transform.position;
        Vector2 colliderSize = aiCharacter.GetColliderSize();

        float distance = Random.Range(minPatrolRange, maxPatrolRange);
        float movingDirection = Mathf.Sign(Random.Range(0, 2) * 2 - 1); // INFO: -1 or 1

        aiCharacter.FaceDirection(movingDirection);

        // INFO: Adjust the destination to be relative to the character's position
        destinationX = aiPosition.x + (distance * movingDirection);

        // INFO: Check for Walls and Adjust the Destination
        RaycastHit2D hit = Physics2D.Raycast(aiPosition, Vector2.right * movingDirection, distance + colliderSize.x, aiCharacter.BoundaryMask);

        if (hit.collider != null)
        {
            // INFO: If there is a wall, set the destination to the wall position less the character's size
            destinationX = hit.point.x - (colliderSize.x * movingDirection);

            // INFO: If the character is too close to the wall, invert the direction
            if (Mathf.Abs(destinationX - aiPosition.x) <= colliderSize.x)
            {
                movingDirection *= -1;
                aiCharacter.FaceDirection(movingDirection);
                destinationX = aiPosition.x + (distance * movingDirection);
            }
        }
    }

    private void DetectTarget()
    {
        if (aiCharacter.Target == null || aiCharacter.TargetIsDead) { return; }

        // INFO: Target Detection Logic
        if (Physics2D.OverlapBox(aiCharacter.transform.position, targetDetectionBox, 0.0f, aiCharacter.TargetMask) != null)
        {
            aiCharacter.SwitchState<GroundChaseState>();
        }
    }

    private bool HasReachedDestination()
    {
        return Mathf.Abs(aiCharacter.transform.position.x - destinationX) <= destinationDifference;
    }
}
