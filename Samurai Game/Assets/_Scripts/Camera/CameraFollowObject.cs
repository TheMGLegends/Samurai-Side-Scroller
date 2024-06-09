using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [Header("Follow Settings:")]
    [SerializeField] private Transform target;

    [Header("Flip Settings:")]
    [SerializeField] private float flipYRotationDuration;

    private bool isFacingRight;

    private void Awake()
    {
        if (target.rotation.y == 0)
        {
            isFacingRight = true;
        }
        else
        {
            isFacingRight = false;
        }
    }

    private void Update()
    {
        transform.position = target.position;
    }

    public void Turn()
    {
        LeanTween.rotateY(gameObject, DetermineEndRotation(), flipYRotationDuration).setEaseInOutSine();
    }

    private float DetermineEndRotation()
    {
        isFacingRight = !isFacingRight;

        if (isFacingRight)
        {
            return 0f;
        }
        else
        {
            return 180f;
        }
    }
}
