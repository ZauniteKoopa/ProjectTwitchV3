using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownShooterAttackController : IAttackModule
{
    // Reference variables to components of player package
    [Header("Reference variables")]
    [SerializeField]
    private Camera playerCamera = null;
    [SerializeField]
    private Transform playerCharacter = null;
    [SerializeField]
    private Transform primaryBullet = null;

    // Stats for attacking (move to Player Status)
    [Header("Primary Attack Stats")]
    [SerializeField]
    private float primaryAttackRate = 0.65f;
    [SerializeField]
    private float primaryBulletSpeed = 20f;
    [SerializeField]
    private float primaryAttackMoveReduction = 0.6f;

    // Variables for aiming
    private Plane aimPlane;
    private Vector2 inputMouseCoordinates;
    private Vector3 aimForward;

    // Variables for firingPrimaryAttack
    private bool firingPrimaryAttack = false;
    private bool primaryAttackSequenceRunning = false;


    // On awake, error check and initialize variables
    private void Awake() {
        aimPlane = new Plane(Vector3.up, playerCharacter.position);
    }


    // Invokable function to create a projectile at the specified direction
    //  Pre: player is holding left click (firingPrimaryAttack is true)
    private IEnumerator primaryAttackSequence() {
        // Set flag to true
        primaryAttackSequenceRunning = true;

        // Keep firing projectiles until you stopped holding left click
        while (firingPrimaryAttack) {
            // Create projectile
            Transform currentProjectile = Object.Instantiate(primaryBullet, playerCharacter.position, Quaternion.identity);
            AbstractStraightProjectile projBehav = currentProjectile.GetComponent<AbstractStraightProjectile>();
            Vector3 currentProjectileDir = getWorldAimLocation() - playerCharacter.position;
            projBehav.setUpMovement(currentProjectileDir, primaryBulletSpeed);

            // Wait for attack rate to finish
            yield return new WaitForSeconds(primaryAttackRate);
        }

        // Set flag to false once sequence ends
        primaryAttackSequenceRunning = false;
    }



    // Event handler method for when mouse position changes
    public void onAimPositionChange(InputAction.CallbackContext value) {
        inputMouseCoordinates = value.ReadValue<Vector2>();
    }

    
    // Event handler method for when primary fire button click / removed
    public void onPrimaryButtonAction(InputAction.CallbackContext value) {

        // Only run the sequence if this is the first bullet
        if (value.started && !primaryAttackSequenceRunning) {
            firingPrimaryAttack = true;
            StartCoroutine(primaryAttackSequence());
        
        // Once left click is canceled, turn mouse hold flag off
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

    
    // Function to return movement speed factor affected by this attack module
    //  Pre: none
    //  Post: returns a float that tells how much movement speed should be reduced by currently
    public override float getMovementSpeedFactor() {
        return (primaryAttackSequenceRunning) ? primaryAttackMoveReduction : 1.0f;
    }


    // Function to get the new forward calculated by this attack module
    //  Pre: newForward needs to be any vector3
    //  Post: returns whether forward should be overriden and puts overriden forward into newForward
    public override bool getNewForward(out Vector3 newForward) {
        newForward = Vector3.zero;
        if (!primaryAttackSequenceRunning) {
            return false;
        }

        newForward = getWorldAimLocation() - playerCharacter.position;
        newForward = newForward.normalized;
        return true;
    }
}
