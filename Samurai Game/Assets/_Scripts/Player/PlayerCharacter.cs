using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerActionController))]
[RequireComponent(typeof(PlayerHealthController))]
[RequireComponent(typeof(PlayerAnimationController))]

public class PlayerCharacter : MonoBehaviour
{
    public PlayerInputActions PlayerInputActions { get; private set; }

    public PlayerMovementController PlayerMovementController { get; private set; }
    public PlayerActionController PlayerActionController { get; private set; }
    public PlayerHealthController PlayerHealthController { get; private set; }
    public PlayerAnimationController PlayerAnimationController { get; private set; }

    private void Awake()
    {
        PlayerInputActions = new PlayerInputActions();

        PlayerMovementController = GetComponent<PlayerMovementController>();
        PlayerMovementController.Init(this);

        PlayerActionController = GetComponent<PlayerActionController>();
        PlayerActionController.Init(this);

        PlayerHealthController = GetComponent<PlayerHealthController>();
        PlayerHealthController.Init(this);

        PlayerAnimationController = GetComponent<PlayerAnimationController>();
        PlayerAnimationController.Init(this);
    }

}
