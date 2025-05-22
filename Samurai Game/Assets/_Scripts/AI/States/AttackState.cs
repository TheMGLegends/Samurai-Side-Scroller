using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AttackState : State
{
    [Header("Gizmo Toggles")]

    [SerializeField] private bool drawTargetLoseBox = true;


    [Header("Attack Settings")]

    [Range(0, 100)]
    [SerializeField] private float comboChance = 25.0f;


    [Header("Cooldown Settings")]

    [Min(0)]
    [SerializeField] private float attackCooldown = 0.25f;


    [Header("Lose Target Settings")]

    [Min(0)]
    [SerializeField] private Vector2 targetLoseBox = Vector2.zero;


    [Header("Defend Settings")]

    [Range(0, 100)]
    [SerializeField] private float defendChance = 10.0f;


    private readonly List<GameObject> attackObjects = new();
    private int currentAttackIndex = 0;
    private string currentAttackName = string.Empty;
    private Coroutine attackCooldownCoroutine = null;
    private bool isOnCooldown = false;


    private void Reset()
    {
        // INFO: Add Objects and Components to the AICharacter
        if (transform.parent.Find("Attacks") == null)
        {
            GameObject attacks = new("Attacks");
            Undo.RegisterCreatedObjectUndo(attacks, "Create Attacks");
            Undo.SetTransformParent(attacks.transform, transform.parent, "Set Attacks Parent");

            // INFO: Add First Attack
            if (attacks.transform.Find("Attack1") == null)
            {
                GameObject attack1 = new("Attack1");
                Undo.RegisterCreatedObjectUndo(attack1, "Create Attack1");
                Undo.SetTransformParent(attack1.transform, attacks.transform, "Set Attack1 Parent");

                // INFO: Add Components
                PolygonCollider2D polygonCollider = Undo.AddComponent<PolygonCollider2D>(attack1);
                polygonCollider.isTrigger = true;
                polygonCollider.enabled = false;
                Undo.AddComponent<AttackAction>(attack1);
            }

            // INFO: Add Second Attack
            if (attacks.transform.Find("Attack2") == null)
            {
                GameObject attack2 = new("Attack2");
                Undo.RegisterCreatedObjectUndo(attack2, "Create Attack2");
                Undo.SetTransformParent(attack2.transform, attacks.transform, "Set Attack2 Parent");

                // INFO: Add Components
                PolygonCollider2D polygonCollider = Undo.AddComponent<PolygonCollider2D>(attack2);
                polygonCollider.isTrigger = true;
                polygonCollider.enabled = false;
                Undo.AddComponent<AttackAction>(attack2);
            }
        }
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        // INFO: Remove Objects and Components added by this state from the AICharacter
        foreach (GameObject attackObject in attackObjects)
        {
            Undo.DestroyObjectImmediate(attackObject);
        }

        attackObjects.Clear();
#endif
    }

    private void OnDrawGizmosSelected()
    {
        if (drawTargetLoseBox)
        {
            // INFO: Draw the Target Lose Box
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position, new Vector3(targetLoseBox.x, targetLoseBox.y));
        }
    }

    private void Awake()
    {
        // INFO: Get all the attack objects from the AICharacter
        Transform attacks = transform.parent.Find("Attacks");

        if (attacks != null)
        {
            for (int i = 0; i < attacks.childCount; i++)
            {
                GameObject attackObject = attacks.GetChild(i).gameObject;
                attackObjects.Add(attackObject);
            }
        }
    }

    public override void Enter()
    {
        // INFO: Subscribe to the player on attack event
        if (TryGetComponent<ShieldState>(out _) && aiCharacter.Target.TryGetComponent(out PlayerActionController playerActionController))
        {
            playerActionController.OnAttackEvent += BlockAttack;
        }

        // INFO: Prevent pathfinding character from moving
        if (aiCharacter.AIPath != null) { aiCharacter.AIPath.canMove = false; }

        attackCooldownCoroutine = StartCoroutine(AttackCooldownCoroutine());
    }

    public override void Run()
    {
        if (isOnCooldown) { return; }

        if (!aiCharacter.AnimatorIsPlaying(currentAttackName))
        {
            // INFO: Combo Chance
            if (Random.Range(0, 100) <= comboChance)
            {
                NextAttack();
                aiCharacter.PlayAnimation(currentAttackName);
            }
            else if (!DetectTargetLost())
            {
                attackCooldownCoroutine = StartCoroutine(AttackCooldownCoroutine());
            }
        }
    }

    public override void Exit()
    {
        // INFO: Unsubscribe from the player on attack event
        if (TryGetComponent<ShieldState>(out _) && aiCharacter.Target.TryGetComponent(out PlayerActionController playerActionController))
        {
            playerActionController.OnAttackEvent -= BlockAttack;
        }

        if (attackCooldownCoroutine != null)
        {
            StopCoroutine(attackCooldownCoroutine);
            attackCooldownCoroutine = null;
        }

        isOnCooldown = false;
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        isOnCooldown = true;
        aiCharacter.PlayAnimation("Idle");

        yield return new WaitForSeconds(attackCooldown);

        aiCharacter.FaceDirection(Mathf.Sign(aiCharacter.Target.transform.position.x - aiCharacter.transform.position.x));
        ChooseAttack();
        aiCharacter.PlayAnimation(currentAttackName);
        isOnCooldown = false;
    }

    private bool DetectTargetLost()
    {
        if (Physics2D.OverlapBox(transform.position, targetLoseBox, 0, aiCharacter.TargetMask) == null)
        {
            if (transform.TryGetComponent<GroundChaseState>(out _))
            {
                aiCharacter.SwitchState<GroundChaseState>();
            }
            else
            {
                aiCharacter.SwitchState<AirChaseState>();
            }

            return true;
        }

        return false;
    }

    private void ChooseAttack()
    {
        // INFO: Choose a random attack from the list of attacks
        currentAttackIndex = Random.Range(0, attackObjects.Count);
        currentAttackName = attackObjects[currentAttackIndex].name;
    }

    private void NextAttack()
    {
        // INFO: Choose the next attack in the list of attacks
        currentAttackIndex++;
        if (currentAttackIndex >= attackObjects.Count)
            currentAttackIndex = 0;
        currentAttackName = attackObjects[currentAttackIndex].name;
    }

    private void BlockAttack()
    {
        // INFO: Block the attack
        if (Random.Range(0, 100) <= defendChance)
        {
            aiCharacter.SwitchState<ShieldState>();
        }
    }
}
