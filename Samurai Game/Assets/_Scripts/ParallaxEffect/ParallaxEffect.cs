using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [SerializeField] private Camera playerCamera; // TEMPORARY SOLUTION UNTIL REFERENCE MANAGER
    [SerializeField] private float parallaxEffect;

    private float length;
    private float startPos;

    private void Start()
    {
        startPos = transform.position.x;

        if (TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            length = spriteRenderer.bounds.size.x;
        }
    }

    private void Update()
    {
        //Temp relates to where the camera is based on each objects parallax 
        float temp = playerCamera.transform.position.x * (1 - parallaxEffect);

        //Distance is the offset at which each object travels at from the camera based on its level of parallax 
        float distance = playerCamera.transform.position.x * parallaxEffect;

        //The new position is added onto the objects starting position to maintain smooth and constant object movement
        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

        //Given that temp is greater than the size of the objects image we need to move the object forwards so that the player can still see it
        if (temp > startPos + length)
        {
            startPos += length * 2;
        }
        else if (temp < startPos - length)
        {
            startPos -= length * 2;
        }
    }
}
