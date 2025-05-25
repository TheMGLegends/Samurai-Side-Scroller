using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AirChaseState : State
{
    [Header("Gizmo Toggles")]

    [SerializeField] private bool drawAttackRangeRadius = true;


    [Header("Chase Settings")]

    [Min(0)]
    [SerializeField] private float chaseSpeed = 1.5f;


    [Header("Attack Settings")]

    [Min(0)]
    [SerializeField] private float attackRangeRadius = 1.0f;


    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        // INFO: Draw the Attack Range Radius
        if (drawAttackRangeRadius)
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(transform.position, Vector3.forward, attackRangeRadius);
        }
#endif
    }

    public override void Enter()
    {
        aiCharacter.PlayAnimation("Chase");
        aiCharacter.AIPath.maxSpeed = chaseSpeed;
        aiCharacter.AIPath.canMove = true;
    }

    public override void Run()
    {
        TargetWithinAttackRange();
        aiCharacter.FaceDirection();
    }

    private void TargetWithinAttackRange()
    {
        if (Physics2D.OverlapCircle(transform.position, attackRangeRadius, aiCharacter.TargetMask) != null)
        {
            aiCharacter.SwitchState<AttackState>();
        }
    }
}
