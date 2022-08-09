using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component used to maintain constant rotation even if the parent transform is rotating
public class ConstantRotation : MonoBehaviour
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
