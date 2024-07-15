using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class ChestController : MonoBehaviour, IInteractable
{
    [Header("Chest Settings:")]
    [ReadOnlyInspector][SerializeField] private bool isOpened;

    private BoxCollider2D boxCollider2D;
    private Animator animator;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        isOpened = !isOpened;
        animator.SetBool("IsOpened", isOpened);
    }
}
