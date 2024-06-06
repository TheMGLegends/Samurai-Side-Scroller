using UnityEngine;


/// <summary>
/// Contains Data for an Event
/// </summary>
public struct EventData
{
    public GameObject Instigator { get; private set; }
    public GameObject Target { get; private set; }
    public string AnimationState { get; private set; }
    public float AttackDamage { get; private set; }

    /// <summary>
    /// Generic GameObject EventData Constructor
    /// </summary>
    /// <param name="_instigator">The GO that has instigated the event</param>
    public EventData(GameObject _instigator)
    {
        Instigator = _instigator;
        Target = null;
        AnimationState = "";
        AttackDamage = 0;
    }

    /// <summary>
    /// Animation EventData Constructor
    /// </summary>
    /// <param name="_animationState">The new animation state</param>
    public EventData(string _animationState)
    {
        Instigator = null;
        Target = null;
        AnimationState = _animationState;
        AttackDamage = 0;
    }

    /// <summary>
    /// Attack Damage EventData Constructor
    /// </summary>
    /// <param name="_instigator">The GO that has instigated the event</param>
    /// <param name="_target">The GO that is affected by the event</param>
    /// <param name="_attackDamage">The damage dealt</param>
    public EventData(GameObject _instigator, GameObject _target, float _attackDamage)
    {
        Instigator = _instigator;
        Target = _target;
        AnimationState = "";
        AttackDamage = _attackDamage;
    }
}

/// <summary>
/// Interface for all classes that will observe a subject
/// </summary>
public interface IObserver
{
    /// <summary>
    /// Called by the subject when an event occurs
    /// </summary>
    /// <param name="eventType">The type of event that has occured</param>
    /// <param name="gameObject">The game object that this event affects</param>
    public void OnNotify(EventType eventType, EventData eventData);
}
