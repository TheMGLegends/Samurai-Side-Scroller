using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerActionController))]
[RequireComponent(typeof(PlayerHealthController))]

public class PlayerCharacter : MonoBehaviour
{
    public PlayerInputActions PlayerInputActions { get; private set; }

    private PlayerMovementController playerMovementController;
    private PlayerActionController playerActionController;
    private PlayerHealthController playerHealthController;

    private void Awake()
    {
        PlayerInputActions = new PlayerInputActions();

        playerMovementController = GetComponent<PlayerMovementController>();
        playerMovementController.Init(this);

        playerActionController = GetComponent<PlayerActionController>();
        playerActionController.Init(this);

        playerHealthController = GetComponent<PlayerHealthController>();
        playerHealthController.Init(this);
    }

}
