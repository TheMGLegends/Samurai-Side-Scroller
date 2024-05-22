using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]

/// <summary>
/// Base class for all types of animation controllers
/// </summary>
public abstract class BaseAnimationController<T> : MonoBehaviour, IObserver
    where T : System.Enum // MIGHT NOT WORK TEST LATER
{
    [Header("Subjects:")]
    [SerializeField] private List<Subject> subjects = new();

    [Space(10)]

    [Header("Animation Settings:")]
    [SerializeField] private List<T> animationStrings = new();

    private Animator animator;
    private T currentState;

    protected void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public abstract void OnNotify(EventType eventType, GameObject gameObject);
}
