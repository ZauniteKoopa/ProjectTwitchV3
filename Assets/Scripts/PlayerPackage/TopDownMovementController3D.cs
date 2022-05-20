using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;

public class TopDownMovementController3D : MonoBehaviour
{
    // Movement variables
    [SerializeField]
    private float movementSpeed = 6.0f;
    private bool isMoving = false;
    private Vector2 inputVector = Vector2.zero;
    private Vector3 movementForward = Vector3.forward;

    // Reference variables
    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private Transform playerCharacterTransform;


    // FixedUpdate function: runs every frame
    private void FixedUpdate() {
        if (isMoving) {
            handleMovement(Time.fixedDeltaTime);
        }

        setFacingDirection();
    }

    // Main method to handle movement
    //  Pre: deltaTime > 0, inputVector is a normalized vector (NOT THE ZERO VECTOR), cameraTransform != null
    //  Post: Translates the transform of the player based on the input vector and the camera direction
    private void handleMovement(float deltaTime) {
        // Preconditions
        Debug.Assert(inputVector.magnitude > 0.999f && inputVector.magnitude <= 1.001f);
        Debug.Assert(deltaTime > 0.0f);
        Debug.Assert(cameraTransform != null);

        // Get input axis values
        float inputX = inputVector.x;
        float inputY = inputVector.y;

        // Get camera axis values
        Vector3 forwardVector = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
        Vector3 rightVector = Vector3.Cross(Vector3.up, forwardVector);

        // Get directional vector
        Vector3 movementWorldDir = (inputX * rightVector) + (inputY * forwardVector);
        movementWorldDir.Normalize();

        // Apply directional vector to current transform
        transform.Translate(movementWorldDir * movementSpeed * deltaTime, Space.World);
        movementForward = movementWorldDir;
    }

    // Main method to determine where the character is facing, considering both movement and aim
    //  Pre: movementForward and aim direction is a vector that's not the zero vector, playerTransform != null
    //  Post: Sets the facing direction of the player character, assuming that player always face to its relative +Z dir
    private void setFacingDirection() {
        Debug.Assert(playerCharacterTransform != null);

        playerCharacterTransform.forward = movementForward;
    }

    // Event handler for 4 axis movement
    public void onInputVectorChange(InputAction.CallbackContext value) {
        // Set flag for whether player is pressing button
        isMoving = !value.canceled;

        // Set inputVector value
        Vector3 eventVector = value.ReadValue<Vector2>();
        inputVector = eventVector;
    }
}
