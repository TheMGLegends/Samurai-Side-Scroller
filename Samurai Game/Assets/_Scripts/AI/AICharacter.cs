using Pathfinding;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class AICharacter : MonoBehaviour
{
    [Header("AI Customization")]
    [Space(5)]

    [SerializeField] private bool usePathfinding = true;

    [TypeFilter(typeof(State))] public SerializableType CurrentState;
    private State currentState;

    /// <summary>
    /// User-defined states that the AI has access to
    /// </summary>
    [TypeFilter(typeof(State))] [SerializeField] private List<SerializableType> States;
    private readonly Dictionary<Type, State> states = new();

    public AIPath AIPath { get; private set; }
    public Seeker Seeker { get; private set; }
    public AIDestinationSetter AIDestinationSetter { get; private set; }
    public Animator Animator { get; private set; }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (usePathfinding)
        {
            EditorApplication.delayCall += () =>
            {
                // INFO: AIPath auto adds Seeker
                if (GetComponent<AIPath>() == null)
                {
                    AIPath path = Undo.AddComponent<AIPath>(gameObject);
                    path.orientation = OrientationMode.YAxisForward;
                    path.enableRotation = false;
                }

                if (GetComponent<AIDestinationSetter>() == null)
                {
                    Undo.AddComponent<AIDestinationSetter>(gameObject);
                }
            };
        }
        else
        {
            EditorApplication.delayCall += () =>
            {
                // INFO: Remove the components if they exist
                if (GetComponent<AIPath>() != null)
                {
                    Undo.DestroyObjectImmediate(GetComponent<AIPath>());
                }

                if (GetComponent<Seeker>() != null)
                {
                    Undo.DestroyObjectImmediate(GetComponent<Seeker>());
                }

                if (GetComponent<AIDestinationSetter>() != null)
                {
                    Undo.DestroyObjectImmediate(GetComponent<AIDestinationSetter>());
                }
            };
        }
#endif
    }

    private void Awake()
    {
        AIPath = GetComponent<AIPath>();
        Seeker = GetComponent<Seeker>();
        AIDestinationSetter = GetComponent<AIDestinationSetter>();
        Animator = GetComponent<Animator>();

        InitialiseStates();
    }

    private void Update()
    {
        if (currentState != null) { currentState.Run(); }

        FaceMovingDirection();
    }

    private void InitialiseStates()
    {
        // INFO: Create a holder for the states
        GameObject statesHolder = new("States");
        statesHolder.transform.SetParent(transform);

        // INFO: Add the states to the holder
        foreach (var state in States)
        {
            if (statesHolder.GetComponent(state.Type) != null)
            {
                Debug.LogWarning($"State {state} already exists on {gameObject.name}");
                continue;
            }

            State stateComponent = (State)statesHolder.AddComponent(state.Type);
            stateComponent.SetAICharacter(this);

            // INFO: Add the state to the dictionary with the type as the key
            states.Add(state.Type, stateComponent);
        }

        // INFO: Set the current state
        if (states.ContainsKey(CurrentState.Type))
        {
            currentState = states[CurrentState.Type];
            currentState.Enter();
        }
    }

    private void FaceMovingDirection()
    {
        // TODO: Need to remake this to be in a state maybe and also not use AI stuff if no ai found

        if (AIPath == null) { return; }

        if (AIPath.desiredVelocity.x >= 0.01f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (AIPath.desiredVelocity.x <= -0.01f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    public void SwitchState<T>() where T : State
    {
        Type type = typeof(T);

        if (states.ContainsKey(type))
        {
            if (currentState != null) { currentState.Exit(); }

            CurrentState = type;
            currentState = states[CurrentState];
            currentState.Enter();
        }
        else
        {
            Debug.LogError($"State {type} not found in states list");
            return;
        }
    }
}
