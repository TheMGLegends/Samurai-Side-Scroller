using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

/// <summary>
/// Base class for all types of animation controllers
/// </summary>
public abstract class BaseAnimationController<T> : MonoBehaviour, IObserver
    where T : System.Enum
{
    [Header("Subjects:")]
    [SerializeField] private List<Subject> subjects = new();

    private Animator animator;
    private string currentState;

    protected void Awake()
    {
        animator = GetComponent<Animator>();

        // INFO: Adds this observer to all subjects
        foreach (Subject subject in subjects)
            subject.AddObserver(this);
    }

    public void ChangeAnimationState(string newState, float animationSpeed = 1.0f)
    {
        // INFO: Prevents the same animation from being played again from the start
        if (currentState == newState)
            return;

        animator.Play(newState);
        animator.speed = animationSpeed;
        currentState = newState;
    }

    public abstract void OnNotify(EventType eventType, EventData eventData);
}
