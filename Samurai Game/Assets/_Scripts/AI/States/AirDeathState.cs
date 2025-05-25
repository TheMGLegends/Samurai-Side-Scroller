using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AirDeathState : State
{
    [Header("Gizmo Toggles")]

    [SerializeField] private bool drawGroundDetectionPoint = true;


    [Header("Death Settings")]

    [Tooltip("The sprite to display when the character is falling")]
    [SerializeField] private Sprite fallingSprite;

    [SerializeField] private float fallingSpeed = 1.0f;

    [SerializeField] private Sprite deathSprite;


    [Header("Ground Detection Settings")]

    [SerializeField] private Vector2 groundDetectionPoint = Vector2.zero;

    [SerializeField] private LayerMask groundMask;

    
    private bool isGrounded = false;
    private bool hasDied = true;


    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (drawGroundDetectionPoint)
        {
            Handles.color = Color.blue;
            Handles.DrawSolidDisc(transform.position + (Vector3)groundDetectionPoint, Vector3.forward, 0.1f);
        }
#endif
    }

    public override void Enter()
    {
        aiCharacter.Animator.enabled = false;
        aiCharacter.SpriteRenderer.sprite = fallingSprite;
    }

    public override void Run()
    {
        IsGrounded();

        if (!isGrounded)
        {
            aiCharacter.transform.position += fallingSpeed * Time.deltaTime * Vector3.down;
        }
        else
        {
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

    private void IsGrounded()
    {
        if (isGrounded) { return; }

        float distance = Mathf.Abs(aiCharacter.transform.position.y - (transform.position + (Vector3)groundDetectionPoint).y);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, distance, groundMask);

        if (hit)
        {
            isGrounded = true;
            AudioManager.Instance.PlaySFX("Fallen", 1.0f, true, hit.point);
            aiCharacter.Animator.enabled = true;
            aiCharacter.PlayAnimation("Death");
        }
    }
}
