using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Function to keep world UI sprites from facing the camera, no matter what
public class WorldUISprite : MonoBehaviour
{
    // Constant forward
    private Vector3 rotationForward;

    // Start is called before the first frame update
    void Awake()
    {
        rotationForward = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        transform.forward = rotationForward;
    }
}
