using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownShooterAttackController : MonoBehaviour
{
    // Reference variables to components of player package
    [SerializeField]
    private Camera playerCamera = null;
    [SerializeField]
    private Transform playerCharacter = null;
    [SerializeField]
    private Transform primaryBullet = null;

    // Variables for aiming
    private Plane aimPlane;
    private Vector2 inputMouseCoordinates;
    private bool firingPrimaryAttack = false;


    // On awake, error check and initialize variables
    private void Awake() {
        aimPlane = new Plane(Vector3.up, playerCharacter.position);
    }


    // Event handler method for when mouse position changes
    public void onAimPositionChange(InputAction.CallbackContext value) {
        inputMouseCoordinates = value.ReadValue<Vector2>();
    }

    
    // Event handler method for when primary fire button click / removed
    public void onPrimaryButtonAction(InputAction.CallbackContext value) {
        if (value.started) {
            firingPrimaryAttack = true;
            primaryBullet.position = getWorldAimLocation();
        } else if (value.canceled) {
            firingPrimaryAttack = false;
        }
    }


    // Event handler method for when secondary fire button click
    public void onSecondaryButtonClick(InputAction.CallbackContext value) {
        if (value.started) {
            Debug.Log("Fire grenade");
        }
    }

    
    // Private helper method to get world aim location
    //  Does so by creating a ray on mouse position on camera and have it intersect the aim plane
    private Vector3 getWorldAimLocation() {
        Ray inputRay = playerCamera.ScreenPointToRay(inputMouseCoordinates);
        float intersectionDist = 0.0f;

        aimPlane.Raycast(inputRay, out intersectionDist);
        return inputRay.GetPoint(intersectionDist);

    }
}
