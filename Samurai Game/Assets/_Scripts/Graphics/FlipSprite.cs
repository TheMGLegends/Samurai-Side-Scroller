using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// OLD CLASS CAN BE DELETED AFTER
public class FlipSprite : MonoBehaviour
{
    [SerializeField] private AIPath aiPath;

    private void Update()
    {
        if (aiPath.desiredVelocity.x >= 0.01f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (aiPath.desiredVelocity.x <= -0.01f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }
}
