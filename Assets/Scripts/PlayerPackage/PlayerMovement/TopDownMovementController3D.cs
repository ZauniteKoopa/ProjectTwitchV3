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

    // Collision sensors
    [SerializeField]
    private IBlockerSensor frontSensor;
    [SerializeField]
    private IBlockerSensor backSensor;
    [SerializeField]
    private IBlockerSensor leftSensor;
    [SerializeField]
    private IBlockerSensor rightSensor;


    // FixedUpdate function: runs every frame
    private void FixedUpdate() {
        if (isMoving) {
            handleMovement(Time.fixedDeltaTime);
        }

        setFacingDirection();
    }

    // Main method to handle movement
    //  Pre: deltaTime > 0, inputVector is a normalized vector (NOT THE ZERO VECTOR), cameraTransform != null
    //  Post: Translates / rotates the transform of the player based on the input vector and the camera direction
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

        // Get directional forward vector
        Vector3 forwardWorldDir = (inputX * rightVector) + (inputY * forwardVector);
        forwardWorldDir.Normalize();

        // Get movement vector by checking sensors
        float movementX = (inputX < 0 && leftSensor.isBlocked()) ? 0 : inputX;
        movementX = (movementX > 0 && rightSensor.isBlocked()) ? 0 : movementX;
        float movementY = (inputY < 0 && backSensor.isBlocked()) ? 0 : inputY;
        movementY = (movementY > 0 && frontSensor.isBlocked()) ? 0 : movementY;
        Vector3 movementWorldDir = (movementX * rightVector) + (movementY * forwardVector);

        // Translate via the movement vector and change facing direction via the forward vector
        transform.Translate(movementWorldDir * movementSpeed * deltaTime, Space.World);
        movementForward = forwardWorldDir;
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
