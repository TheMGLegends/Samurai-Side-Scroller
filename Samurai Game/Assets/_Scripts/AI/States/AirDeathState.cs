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


    [Header("Ground Detection Settings")]

    [SerializeField] private Vector2 groundDetectionPoint = Vector2.zero;

    [SerializeField] private LayerMask groundMask;

    
    private bool isGrounded = false;
    private bool hasDied = true;


    private void OnDrawGizmosSelected()
    {
        if (drawGroundDetectionPoint)
        {
            Handles.color = Color.blue;
            Handles.DrawSolidDisc(transform.position + (Vector3)groundDetectionPoint, Vector3.forward, 0.1f);
        }
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
                return;
            }

            if (!aiCharacter.AnimatorIsPlaying("Death"))
            {
                aiCharacter.Deactivate();
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
            aiCharacter.Animator.enabled = true;
            aiCharacter.PlayAnimation("Death");
        }
    }
}
