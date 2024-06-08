using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

/// <summary>
/// Base class for all types of animation controllers
/// </summary>
public class AnimationController : MonoBehaviour, IObserver
{
    [Header("Subjects:")]
    [SerializeField] private List<Subject> subjects = new();

    private Animator animator;
    private string currentState;

    #region UnityMethods
    protected void Awake()
    {
        animator = GetComponent<Animator>();

        // INFO: Adds this observer to all subjects
        foreach (Subject subject in subjects)
            subject.AddObserver(this);
    }
    #endregion UnityMethods

    #region AnimationMethods
    public void ChangeAnimationState(string newState)
    {
        // INFO: Prevents the same animation from being played again from the start
        if (currentState == newState)
            return;

        animator.Play(newState);
        currentState = newState;
    }
    #endregion AnimationMethods

    public void OnNotify(EventType eventType, EventData eventData)
    {
        if (eventType == EventType.AnimationStateChange)
        {
            ChangeAnimationState(eventData.AnimationState);
        }
    }
}
