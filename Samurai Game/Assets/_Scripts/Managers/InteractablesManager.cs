using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractablesManager : MonoBehaviour
{
    [Header("Interactables Settings:")]

    [ReadOnlyInspector]
    [SerializeField] private int interactableCount = 0;
    [SerializeField] private float resetInteractableDelay = 30.0f;

    private readonly List<IInteractable> interactables = new();
    private readonly List<Coroutine> resetInteractableCoroutines = new();
    private PlayerHealthController playerHealthController;

    private void Start()
    {
        // INFO: Find all interactables in the scene
        IInteractable[] foundInteractables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IInteractable>().ToArray();

        foreach (IInteractable interactable in foundInteractables)
        {
            interactables.Add(interactable);
            interactable.OnInteractedEvent += (interactable) => ResetInteractable(interactable);
        }

        interactableCount = interactables.Count;

        // INFO: Subscribe to OnPlayerRespawnEvent
        playerHealthController = FindFirstObjectByType<PlayerHealthController>();

        if (playerHealthController)
        {
            playerHealthController.OnPlayerRespawnEvent += ResetAllInteractables;
        }
    }

    private void OnDestroy()
    {
        foreach (IInteractable interactable in interactables)
        {
            interactable.OnInteractedEvent -= (interactable) => ResetInteractable(interactable);
        }

        foreach (Coroutine coroutine in resetInteractableCoroutines)
        {
            StopCoroutine(coroutine);
        }

        resetInteractableCoroutines.Clear();
        interactables.Clear();
        interactableCount = 0;

        if (playerHealthController)
        {
            playerHealthController.OnPlayerRespawnEvent -= ResetAllInteractables;
        }
    }

    private void ResetInteractable(IInteractable interactable)
    {
        resetInteractableCoroutines.Add(StartCoroutine(ResetInteractableCoroutine(interactable)));
    }

    private IEnumerator ResetInteractableCoroutine(IInteractable interactable)
    {
        yield return new WaitForSeconds(resetInteractableDelay);

        interactable.ResetInteractable();
        resetInteractableCoroutines.Remove(resetInteractableCoroutines.FirstOrDefault(coroutine => coroutine.Equals(interactable)));
    }

    private void ResetAllInteractables()
    {
        foreach (IInteractable interactable in interactables)
        {
            interactable.ResetInteractable();
        }

        resetInteractableCoroutines.Clear();
    }
}
