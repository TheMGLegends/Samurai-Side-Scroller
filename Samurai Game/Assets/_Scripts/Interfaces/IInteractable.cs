using System;
using UnityEngine;

public interface IInteractable
{
    event Action<IInteractable> OnInteractedEvent;

    void Interact(GameObject interactor);
    void ResetInteractable();
}
