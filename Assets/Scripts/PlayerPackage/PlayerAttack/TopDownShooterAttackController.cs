using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class TopDownShooterAttackController : IAttackModule
{
    // Reference variables to components of player package
    [Header("Reference variables")]
    [SerializeField]
    private Camera playerCamera = null;
    [SerializeField]
    private Transform playerCharacter = null;
    [SerializeField]
    private Transform secondaryCask = null;
    private ITwitchStatus twitchPlayerStatus;
    private ITwitchInventory twitchInventory;

    // Stats for attacking (move to Player Status)
    [Header("Primary Attack Stats")]
    [SerializeField]
    private float primaryAttackRate = 0.65f;
    [SerializeField]
    private float primaryBulletSpeed = 20f;
    [SerializeField]
    private float primaryAttackMoveReduction = 0.6f;
    [SerializeField]
    private IAimAssist aimAssist;


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

    // Other modules
    [Header("Other input modules")]
    [SerializeField]
    private UserInterfaceInputModule uiModule;


    // Constaminate zone
    [SerializeField]
    private ContaminateZone contaminateZone;

    // Variables for aiming
    private Vector2 inputMouseCoordinates;

    // Variables for firingPrimaryAttack
    private bool firingPrimaryAttack = false;
    private bool primaryAttackSequenceRunning = false;


    // On awake, error check and initialize variables
    private void Awake() {

        // Do error checking
        if (playerCamera == null) {
            Debug.LogError("Camera not connected to attack package for " + transform, transform);
        } else if (playerCharacter == null) {
            Debug.LogError("Player character not connected to attack package for " + transform, transform);
        } else if (secondaryCask == null) {
            Debug.LogError("Secondary weapon not connected to attack package for " + transform, transform);
        } else if (uiModule == null) {
            Debug.LogError("UI Module not connected to attack package for " + transform, transform);
        }

        // get twitch status
        twitchPlayerStatus = playerCharacter.GetComponent<ITwitchStatus>();
        twitchInventory = playerCharacter.GetComponent<ITwitchInventory>();
        
        if (twitchPlayerStatus == null || twitchInventory == null) {
            Debug.LogError("Player's status or Inventory script component not connected to attack package for " + playerCharacter, playerCharacter);
        }

        // Connect to contaminate zone kill event to stealth reset
        if (contaminateZone != null) {
            contaminateZone.targetKilledEvent.AddListener(twitchPlayerStatus.onStealthReset);
            twitchPlayerStatus.contaminateReadyEvent.AddListener(contaminateZone.onContaminateReady);
            twitchPlayerStatus.contaminateUsedEvent.AddListener(contaminateZone.onContaminateUsed);
        }

        if (aimAssist == null) {
            Debug.LogWarning("Aim assist not connected to attack controller of player. Player might need help aiming. It's a raw direction vector.", transform);
        }
    }


    // Invokable function to create a projectile at the specified direction
    //  Pre: player is holding left click (firingPrimaryAttack is true)
    private IEnumerator primaryAttackSequence() {
        // Set flag to true
        primaryAttackSequenceRunning = true;

        // Keep firing projectiles until you stopped holding left click
        while (firingPrimaryAttack) {

            if (!uiModule.inMenu() && twitchPlayerStatus.canMove()) {
                // Get aim direction
                Vector3 currentProjectileDir = getWorldAimLocation() - playerCharacter.position;
                if (aimAssist != null) {
                    currentProjectileDir = aimAssist.adjustAim(currentProjectileDir, playerCharacter.position);
                }

                // Launch the projectile
                PoisonVial.launchBasicAttack(currentProjectileDir, primaryBulletSpeed, playerCharacter.position, twitchPlayerStatus.getPrimaryVial(), twitchPlayerStatus.getAttackMultiplier());

                // Reduce cost if possible (cost will always either be 1 or 0, no if statement needed)
                twitchPlayerStatus.consumePrimaryVialBullet();
            }

            // Wait for attack rate to finish
            yield return new WaitForSeconds(primaryAttackRate * twitchPlayerStatus.getAttackRateFactor());
        }

        // Set flag to false once sequence ends
        primaryAttackSequenceRunning = false;
    }


    // Invokable function to start cask throwing sequence
    private IEnumerator secondaryAttackSequence(IVial curPoison) {
        caskSequenceRunning = true;
        twitchPlayerStatus.stun(true);

        yield return new WaitForSeconds(caskAnticipationTime);

        // Create cask and launch at cask destination
        Transform currentCask = Object.Instantiate(secondaryCask, playerCharacter.position, Quaternion.identity);
        Vector3 caskDestination = getCaskDestination();
        currentCask.GetComponent<VenomCask>().launch(caskDestination, curPoison);

        twitchPlayerStatus.stun(false);
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
        if (value.started && !caskSequenceRunning && !uiModule.inMenu() && twitchPlayerStatus.canMove()) {
            // Check if you're actually able to throw a cask
            IVial curCask = twitchPlayerStatus.getPrimaryVial();
            bool usedCask = twitchPlayerStatus.consumePrimaryVialCask();

            if (usedCask) {
                caskAimForward = (getWorldAimLocation() - transform.position).normalized;
                StartCoroutine(secondaryAttackSequence(curCask));
            }
        }
    }


    // Event handler for contaminate press
    public void onContaminatePress(InputAction.CallbackContext value) {
        if (value.started && contaminateZone != null && !uiModule.inMenu() && twitchPlayerStatus.canMove()) {
            // Check both twitchPlayerStatus (cooldown) and contaminate zone (in range)
            if (twitchPlayerStatus.willContaminate(contaminateZone.canUseAbility())) {
                contaminateZone.damageAllTargets(0.0f);
            }
        }
    }


    // Event handler for camofladge press
    public void onCamofladgePress(InputAction.CallbackContext value) {
        if (value.started && twitchPlayerStatus.canMove() && !uiModule.inMenu()) {
            bool camofladgeSuccess = twitchPlayerStatus.willCamofladge();
        }
    }


    // Event handler for swap press
    public void onSwapPress(InputAction.CallbackContext value) {
        if (value.started && !uiModule.inMenu()) {
            twitchPlayerStatus.swapVials();
        }
    }


    // Event handler for ultimate press
    public void onUltPress(InputAction.CallbackContext value) {
        if (value.started && twitchPlayerStatus.canMove() && !uiModule.inMenu()) {
            Vector3 ultDest = getCaskDestination();
            twitchPlayerStatus.willExecuteUltimate(ultDest);
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
        Plane aimPlane = new Plane(Vector3.up, playerCharacter.position);

        aimPlane.Raycast(inputRay, out intersectionDist);
        return inputRay.GetPoint(intersectionDist);
    }

    
    // Function to return movement speed factor affected by this attack module
    //  Pre: none
    //  Post: returns a float that tells how much movement speed should be reduced by currently
    public override float getMovementSpeedFactor() {
        if (primaryAttackSequenceRunning) {
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
