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
    [SerializeField] private AudioSource chestOpenSource;
    [SerializeField] private AudioSource chestCloseSource;
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
        chestOpenSource.Play();
        animator.SetBool("IsOpened", isOpened);
        OnInteractedEvent?.Invoke(this);
        onChestOpened?.Invoke();
    }

    public void ResetInteractable()
    {
        isOpened = false;
        chestCloseSource.Play();
        animator.SetBool("IsOpened", isOpened);
    }
}
