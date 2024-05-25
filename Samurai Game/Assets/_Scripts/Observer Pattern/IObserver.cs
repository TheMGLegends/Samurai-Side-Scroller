using UnityEngine;


/// <summary>
/// Contains Data for an Event
/// </summary>
public struct EventData
{
    public GameObject gameObject;
    public string animationState;

    /// <summary>
    /// Generic GameObject EventData Constructor
    /// </summary>
    /// <param name="_gameObject">The GO that has instigated the event</param>
    public EventData(GameObject _gameObject)
    {
        gameObject = _gameObject;
        animationState = "";
    }

    /// <summary>
    /// Animation EventData Constructor
    /// </summary>
    /// <param name="_animationState">The new animation state</param>
    public EventData(string _animationState)
    {
        gameObject = null;
        animationState = _animationState;
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
