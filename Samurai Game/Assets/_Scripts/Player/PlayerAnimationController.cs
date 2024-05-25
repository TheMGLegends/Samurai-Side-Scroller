/// <summary>
/// Player Animation States
/// </summary>
public enum PlayerAnimationStates
{
    None = 0,

    Idle,
    Run,
    Jump,
    Fall,
    Attack1,
    Attack2,
    TakeHit,
    Death
}

/// <summary>
/// Player Animation Controller
/// </summary>
public class PlayerAnimationController : BaseAnimationController<PlayerAnimationStates>
{
    public override void OnNotify(EventType eventType, EventData eventData)
    {
        if (eventType == EventType.PlayerAnimationStateChange)
            ChangeAnimationState(eventData.animationState);
    }
}
