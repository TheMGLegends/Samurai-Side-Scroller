using Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class AICharacter : MonoBehaviour
{
    [SerializeField] private bool usePathfinding;

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
    public Collider2D Collider2D { get; private set; }

    public bool UsePathfinding => usePathfinding;

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

                // INFO: Remove the BoxCollider2D and add a CircleCollider2D
                if (GetComponent<BoxCollider2D>() != null)
                {
                    Undo.DestroyObjectImmediate(GetComponent<BoxCollider2D>());
                }

                if (GetComponent<CircleCollider2D>() == null)
                {
                    CircleCollider2D collider = Undo.AddComponent<CircleCollider2D>(gameObject);
                    collider.isTrigger = true;
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

                // INFO: Remove the CircleCollider2D and add a BoxCollider2D
                if (GetComponent<CircleCollider2D>() != null)
                {
                    Undo.DestroyObjectImmediate(GetComponent<CircleCollider2D>());
                }

                if (GetComponent<BoxCollider2D>() == null)
                {
                    Undo.AddComponent<BoxCollider2D>(gameObject);
                }
            };
        }

        EditorApplication.delayCall += () =>
        {
            CreateStates();
        };
#endif
    }

    private void Awake()
    {
        AIPath = GetComponent<AIPath>();
        Seeker = GetComponent<Seeker>();
        AIDestinationSetter = GetComponent<AIDestinationSetter>();
        Animator = GetComponent<Animator>();
        Collider2D = GetComponent<Collider2D>();
    }

    private void Start()
    {
        InitialiseStates();

        if (currentState != null) { currentState.Enter(); }
    }

    private void Update()
    {
        if (currentState != null) { currentState.Run(); }
    }

    private void CreateStates()
    {
        // INFO: Remove all states from the holder if chosen states is empty
        if (States.Count == 0)
        {
            if (states.Count > 0)
            {
                foreach (var state in states)
                {
                    if (state.Value == null) { continue; }

                    Undo.DestroyObjectImmediate(state.Value);
                }

                states.Clear();
            }

            return;
        }

        // INFO: Create a holder for the states
        GameObject statesHolder = null;

        if (transform.Find("States") == null)
        {
            // INFO: Clear Existing states to avoid duplicate errors
            if (States.Count > 1)
            {
                States.RemoveRange(1, States.Count - 1);
                states.Clear();
            }

            statesHolder = new("States");
            Undo.RegisterCreatedObjectUndo(statesHolder, "Create States Holder");
            Undo.SetTransformParent(statesHolder.transform, transform, "Set States Holder Parent");
        }
        else
        {
            statesHolder = transform.Find("States").gameObject;
        }

        // INFO: Add the states to the holder
        foreach (var state in States)
        {
            if (state.Type == null || statesHolder == null) { continue; }

            State stateComponent = null;

            if (statesHolder.GetComponent(state.Type) == null)
            {
                stateComponent = (State)Undo.AddComponent(statesHolder, state.Type);

                stateComponent.SetAICharacter(this);
                stateComponent.SetUsePathfinding(usePathfinding);
            }
            else
            {
                stateComponent = (State)statesHolder.GetComponent(state.Type);
            }
        }

        // INFO: Go through the states and remove any that are not in the list
        foreach (var state in states)
        {
            if (!States.Any(s => s.Equals(state.Key)))
            {
                Undo.DestroyObjectImmediate(state.Value);
                states.Remove(state.Key);
            }
        }
    }

    private void InitialiseStates()
    {
        // INFO: Register all states in StatesHolder to the states dictionary
        GameObject statesHolder;

        if ((statesHolder = transform.Find("States").gameObject) != null)
        {
            foreach (var state in statesHolder.GetComponents<State>())
            {
                if (state == null) { continue; }

                if (!states.ContainsKey(state.GetType()))
                {
                    state.SetAICharacter(this);
                    state.SetUsePathfinding(usePathfinding);

                    states.Add(state.GetType(), state);
                }
            }

            // INFO: Set the current state
            if (states.ContainsKey(CurrentState.Type))
            {
                currentState = states[CurrentState.Type];
            }
            else
            {
                Debug.LogError($"State {CurrentState.Type} not found in states list");
                return;
            }
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

    public void FaceMovingDirection(float movingDirection = 0.0f)
    {
        // INFO: Sprite Flip based on Pathfinding
        if (AIPath)
        {
            if (AIPath.desiredVelocity.x >= 0.01f)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (AIPath.desiredVelocity.x <= -0.01f)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }
        // INFO: Sprite Flip based on Moving Direction
        else
        {
            if (movingDirection == 1.0f)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (movingDirection == -1.0f)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }
    }

    public void PlayAnimation(string animationName)
    {
        if (Animator == null) { return; }
        Animator.Play(animationName);
    }

    public Vector2 GetColliderSize()
    {
        if (Collider2D != null)
        {
            if (Collider2D is BoxCollider2D boxCollider)
            {
                return boxCollider.size;
            }
        }

        return Vector2.zero;
    }

    public float GetColliderRadius()
    {
        if (Collider2D != null)
        {
            if (Collider2D is CircleCollider2D circleCollider)
            {
                return circleCollider.radius;
            }
        }

        return 0.0f;
    }
}
