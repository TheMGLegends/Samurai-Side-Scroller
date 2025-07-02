using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerActionController))]
[RequireComponent(typeof(PlayerHealthController))]
[RequireComponent(typeof(PlayerAnimationController))]

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]

public class PlayerCharacter : MonoBehaviour
{
    public PlayerInputActions PlayerInputActions { get; private set; }

    public PlayerMovementController PlayerMovementController { get; private set; }
    public PlayerActionController PlayerActionController { get; private set; }
    public PlayerHealthController PlayerHealthController { get; private set; }
    public PlayerAnimationController PlayerAnimationController { get; private set; }

    public Rigidbody2D Rb2D { get; private set; }
    public Animator Animator { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }

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

        Rb2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Wrapper function for calling audio manager to play sfx used mostly in animation events
    /// </summary>
    public void PlaySFX(AnimationEvent animationEvent)
    {
       AudioManager.Instance.PlaySFX(animationEvent.stringParameter, animationEvent.floatParameter);
    }
}
