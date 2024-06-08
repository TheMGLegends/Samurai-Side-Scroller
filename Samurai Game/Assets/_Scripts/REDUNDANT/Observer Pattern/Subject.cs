using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Subject class is the base class for all classes that will be observed
/// </summary>
public abstract class Subject : MonoBehaviour
{
    // INFO: List of Observers
    private readonly List<IObserver> observers = new();

    /// <summary>
    /// Adds an observer to the list of observers
    /// </summary>
    /// <param name="observer">The observer to be added</param>
    public void AddObserver(IObserver observer) => observers.Add(observer);

    /// <summary>
    /// Removes an observer from the list of observers
    /// </summary>
    /// <param name="observer">The observer to be removed</param>
    public void RemoveObserver(IObserver observer) => observers.Remove(observer);

    /// <summary>
    /// Notifies all observers of an event
    /// </summary>
    /// <param name="eventType">The type of event that has occured</param>
    /// <param name="gameObject">The game object that this event affects</param>
    public void NotifyObservers(EventType eventType, EventData eventData)
    {   
        foreach (IObserver observer in observers)
            observer.OnNotify(eventType, eventData);
    }
}
