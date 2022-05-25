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
    [SerializeField]
    private Transform secondaryCask = null;

    // Stats for attacking (move to Player Status)
    [Header("Primary Attack Stats")]
    [SerializeField]
    private float primaryAttackRate = 0.65f;
    [SerializeField]
    private float primaryBulletSpeed = 20f;
    [SerializeField]
    private float primaryAttackMoveReduction = 0.6f;


    // stats for cask throwing
    [Header("Secondary throw stats")]
    [SerializeField]
    private LayerMask caskCollisionMask;
    [SerializeField]
    private float caskThrowWallOffset = 1.0f;
    [SerializeField]
    private float maxCaskThrowDistance = 5.0f;
    [SerializeField]
    private float caskAnticipationTime = 0.4f;
    private bool caskSequenceRunning = false;
    private Vector3 caskAimForward;

    // Variables for aiming
    private Plane aimPlane;
    private Vector2 inputMouseCoordinates;

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


    // Invokable function to start cask throwing sequence
    private IEnumerator secondaryAttackSequence() {
        caskSequenceRunning = true;

        yield return new WaitForSeconds(caskAnticipationTime);

        // Create cask and launch at cask destination
        Transform currentCask = Object.Instantiate(secondaryCask, playerCharacter.position, Quaternion.identity);
        Vector3 caskDestination = getCaskDestination();
        currentCask.GetComponent<VenomCask>().launch(caskDestination);

        caskSequenceRunning = false;
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
        if (value.started && !caskSequenceRunning) {
            caskAimForward = (getWorldAimLocation() - transform.position).normalized;
            StartCoroutine(secondaryAttackSequence());
        }
    }


    // Main function to get valid cask destination via raycast
    //  Pre: none
    //  Post: returns the location where a cask can be thrown without hitting a wall
    private Vector3 getCaskDestination() {
        // Get ray information for raycast
        Vector3 rayDir = getWorldAimLocation() - transform.position;
        float rayDistance = Mathf.Min(rayDir.magnitude, maxCaskThrowDistance);
        rayDir = rayDir.normalized;

        // Shoot out raycast
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(transform.position, rayDir, out hitInfo, rayDistance, caskCollisionMask);
        Vector3 finalDestination = transform.position + (rayDistance * rayDir);

        if (hit) {
            finalDestination = hitInfo.point - (caskThrowWallOffset * rayDir);
        }

        return finalDestination;
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
        if (caskSequenceRunning) {
            return 0.0f;
        } else if (primaryAttackSequenceRunning) {
            return primaryAttackMoveReduction;
        } else {
            return 1.0f;
        }
    }


    // Function to get the new forward calculated by this attack module
    //  Pre: newForward needs to be any vector3
    //  Post: returns whether forward should be overriden and puts overriden forward into newForward
    public override bool getNewForward(out Vector3 newForward) {
        if (primaryAttackSequenceRunning) {
            newForward = getWorldAimLocation() - playerCharacter.position;
            newForward = newForward.normalized;
            return true;

        } else if (caskSequenceRunning) {
            newForward = caskAimForward;
            return true;

        } else {
            newForward = Vector3.zero;
            return false;
        }
    }
}
