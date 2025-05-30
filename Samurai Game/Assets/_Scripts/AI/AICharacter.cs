using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class AICharacter : MonoBehaviour
{
    [Header("Gizmo Toggles")]

    [SerializeField] private bool drawLedgeDetectionPoint = true;


    [Header("State Settings")]

    [SerializeField] private bool usePathfinding;

    [TypeFilter(typeof(State))] public SerializableType CurrentState;
    private State currentState;

    [Tooltip("User-defined states that the AI has access to")]
    [TypeFilter(typeof(State))] [SerializeField] private List<SerializableType> States;
    private readonly Dictionary<Type, State> states = new();


    [Header("Ledge Detection Settings")]

    [Min(0)]
    [Tooltip("The length of the raycast to check for ledges")]
    [SerializeField] private float ledgeRaycastDistance = 1.0f;

    [Min(0)]
    [Tooltip("The offset of the ledge detecting raycast from the characters position")]
    [SerializeField] private float ledgeRaycastOffset = 0.5f;

    [Tooltip("The layers that the character considers as walls")]
    [SerializeField] private LayerMask boundaryMask;


    [Header("Target Settings")]

    [Tooltip("The layers that the character considers as targets")]
    [SerializeField] private LayerMask targetMask;
    private GameObject target;


    [Header("Decay Settings")]

    [Tooltip("The time it takes for the character to decay after being deactivated")]
    [SerializeField] private float decayTime = 2.0f;


    public AIPath AIPath { get; private set; }
    public Seeker Seeker { get; private set; }
    public AIDestinationSetter AIDestinationSetter { get; private set; }
    public Animator Animator { get; private set; }
    public Collider2D Collider2D { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    public bool TargetIsDead { get; private set; }
    public float movingPitch = 1.0f;


    public LayerMask BoundaryMask => boundaryMask;
    public LayerMask TargetMask => targetMask;
    public GameObject Target => target;


    public event Action OnEnemyDeathEvent;


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

    private void OnDestroy()
    {
        // INFO: Unsubscribe from the player death and respawn events
        if (target != null && target.TryGetComponent(out PlayerHealthController healthController))
        {
            healthController.OnPlayerDeathEvent -= OnPlayerDeathResponse;
            healthController.OnPlayerRespawnEvent -= OnPlayerRespawnResponse;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // INFO: Draw the Ledge Detection Maximum Distance Point
        if (drawLedgeDetectionPoint)
        {
            Gizmos.color = Color.blue;
            Vector2 centre = new(transform.position.x + (transform.localScale.x * ledgeRaycastOffset), transform.position.y - ledgeRaycastDistance);
            Gizmos.DrawSphere(centre, 0.1f);
        }
    }

    private void Awake()
    {
        if (usePathfinding)
        {
            AIPath = GetComponent<AIPath>();
            Seeker = GetComponent<Seeker>();
            AIDestinationSetter = GetComponent<AIDestinationSetter>();
        }

        Animator = GetComponent<Animator>();
        Collider2D = GetComponent<Collider2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SetupTarget();
        InitialiseStates();

        if (currentState != null) { currentState.Enter(); }
    }

    private void Update()
    {
        if (currentState != null) { currentState.Run(); }
    }

    private void CreateStates()
    {
#if UNITY_EDITOR
        if (States.Count == 0) { return; }

        // INFO: Remove all states from the holder if chosen states is empty
        if (states.Count > 0)
        {
            foreach (var state in states)
            {
                if (state.Value == null) { continue; }

                Undo.DestroyObjectImmediate(state.Value);
            }

            states.Clear();
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

            if (statesHolder.GetComponent(state.Type) == null)
            {
                Undo.AddComponent(statesHolder, state.Type);
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
#endif
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
                    state.SetStateInfo(this, usePathfinding);
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

    private void SetupTarget()
    {
        target = FindFirstObjectByType<PlayerCharacter>().gameObject;

        if (target != null && target.TryGetComponent(out PlayerHealthController healthController))
        {
            healthController.OnPlayerDeathEvent += OnPlayerDeathResponse;
            healthController.OnPlayerRespawnEvent += OnPlayerRespawnResponse;

            if (usePathfinding)
            {
                AIDestinationSetter.target = target.transform;
            }
        }
    }

    private void OnPlayerDeathResponse()
    {
        TargetIsDead = true; 
        SwitchState<GroundPatrolState>();
    }

    private void OnPlayerRespawnResponse()
    {
        TargetIsDead = false;
    }

    public T SwitchState<T>() where T : State
    {
        Type type = typeof(T);

        if (states.ContainsKey(type))
        {
            if (currentState != null) { currentState.Exit(); }

            CurrentState = type;
            currentState = states[CurrentState];
            currentState.Enter();

            return currentState as T;
        }

        return null;
    }

    public Type GetCurrentState()
    {
        if (currentState == null) { return null; }
        return currentState.GetType();
    }

    /// <summary>
    /// Wrapper function for calling audio manager to play sfx used mostly in animation events
    /// </summary>
    public void PlaySFX(AnimationEvent animationEvent)
    {
        AudioManager.Instance.PlaySFX(animationEvent.stringParameter, animationEvent.floatParameter);
    }

    public void PlayMovingSFX(AnimationEvent animationEvent)
    {
        AudioManager.Instance.PlaySFX(animationEvent.stringParameter, animationEvent.floatParameter, true, transform.position, 2.5f, 7.5f, movingPitch);
    }

    public void FaceDirection(float direction = 0.0f)
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
            if (direction == 1.0f)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (direction == -1.0f)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }
    }

    /// <summary>
    /// Detects if the character is about to fall off a ledge, used by walking characters
    /// <return>True if there is a ledge i.e (If there is no tile to step on)</return>
    /// </summary>
    public bool LedgeDetected()
    {
        Vector2 ledgeRaycastOrigin = new(transform.position.x + (transform.localScale.x * ledgeRaycastOffset), transform.position.y);
        RaycastHit2D floorHit = Physics2D.Raycast(ledgeRaycastOrigin, Vector2.down, ledgeRaycastDistance, boundaryMask);

        // INFO: If the raycast hits nothing, then there must be a ledge
        if (!floorHit) {  return true; }

        return false;
    }

    public void PlayAnimation(string animationName, int layer = 0, float normalizedTime = float.NegativeInfinity)
    {
        if (Animator == null) { return; }
        Animator.Play(animationName, layer, normalizedTime);
    }

    public bool AnimatorIsPlaying()
    {
        if (Animator == null) { return false; }
        return Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }

    public bool AnimatorIsPlaying(string stateName)
    {
        if (Animator == null) { return false; }
        return AnimatorIsPlaying() && !Animator.IsInTransition(0) && Animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    /// <summary>
    /// Get the size of the collider on the character
    /// </summary>
    /// <returns>When CircleCollider2D returns Vector2(radius, radius), otherwise BoxCollider2D Vector2(width, height)</returns>
    public Vector2 GetColliderSize()
    {
        if (Collider2D != null)
        {
            if (Collider2D is BoxCollider2D boxCollider)
            {
                return boxCollider.size;
            }

            if (Collider2D is CircleCollider2D circleCollider)
            {
                return new Vector2(circleCollider.radius, circleCollider.radius);
            }
        }

        return Vector2.zero;
    }

    /// <summary>
    /// Deactivates the AI character and disables all components
    /// </summary>
    public void Deactivate()
    {
        currentState = null;

        Destroy(transform.Find("States").gameObject);
        Destroy(transform.Find("Attacks").gameObject);

        Collider2D.enabled = false;
        Animator.enabled = false;

        StartCoroutine(DecayCoroutine());
    }

    public void InvokeDeathEvent()
    {
        OnEnemyDeathEvent?.Invoke();
    }

    private IEnumerator DecayCoroutine()
    {
        yield return new WaitForSeconds(decayTime);

        Color color = SpriteRenderer.color;

        while (SpriteRenderer.color.a > 0.0f)
        {
            color.a -= Time.deltaTime / decayTime;
            SpriteRenderer.color = color;
            yield return null;
        }

        Destroy(gameObject);
    }
}
