using System;

public interface IInteractable
{
    event Action<IInteractable> OnInteractedEvent;

    void Interact();
    void ResetInteractable();
}
