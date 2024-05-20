using UnityEngine;

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
    public void OnNotify(EventType eventType, GameObject gameObject);
}
