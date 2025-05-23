using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollowScript : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void Update()
    {
        if (target != null)
        {
            transform.position = target.position;
        }
    }
}
