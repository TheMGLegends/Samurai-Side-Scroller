using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class ChestController : MonoBehaviour, IInteractable
{
    [Header("Chest Settings:")]
    [ReadOnlyInspector]
    [SerializeField] private bool isOpened = false;
    [SerializeField] private UnityEvent onChestOpened = new();

    private Animator animator;

    public event Action<IInteractable> OnInteractedEvent;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        if (isOpened) { return; }

        isOpened = true;
        animator.SetBool("IsOpened", isOpened);
        OnInteractedEvent?.Invoke(this);
        onChestOpened?.Invoke();
    }

    public void ResetInteractable()
    {
        isOpened = false;
        animator.SetBool("IsOpened", isOpened);
    }
}
