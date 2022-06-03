using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVisionSensor : MonoBehaviour
{
    // Main variables for vision
    [SerializeField]
    private LayerMask visionMask;
    private ITwitchStatus nearbyTarget;
    private bool playerSpotted = false;

    // Main variables for short term memory
    [SerializeField]
    private float forgetDuration = 1f;
    private Coroutine forgetRoutine;

    // Main method to invoke events onto brain
    [SerializeField]
    private IEnemyBehavior brain;


    // On each fixed update frame, 
    private void FixedUpdate() {
        bool seePlayer = canSeePlayer();

        // Cases where enemy can see the player
        if (seePlayer) {

            // If player wasn't spotted already, set flag to true and trigger event
            if (!playerSpotted) {
                playerSpotted = true;
                brain.onSensedPlayer(nearbyTarget.transform);

            // If player was in the middle of forgetting, stop forget routine
            } else if (forgetRoutine != null) {
                StopCoroutine(forgetRoutine);
                forgetRoutine = null;
            }

        // Case where enemy cannot see the player anymore
        } else {
            if (playerSpotted && forgetRoutine == null) {
                forgetRoutine = StartCoroutine(forgetPlayerSequence());
            }
        }
    }

    
    // Main private sequence to forget unit
    //  Post: sequence will force a delay between not seeing player and the brain forgetting the player exists. Updates any UI associated with this
    private IEnumerator forgetPlayerSequence() {
        yield return new WaitForSeconds(forgetDuration);

        // Forget player
        playerSpotted = false;
        forgetRoutine = null;
        brain.onLostPlayer();
    }


    // Private helper function to see if you can actually see the unit
    //  Post: returns whether or not player is in sight range of enemy AND no objects are blocking
    private bool canSeePlayer() {
        if (nearbyTarget == null) {
            return false;
        }

        // Get information for the ray
        Vector3 targetPosition = nearbyTarget.transform.position;
        Vector3 rayDir = targetPosition - transform.position;
        float rayDist = rayDir.magnitude;
        rayDir.Normalize();

        // Return whether or not ray cast dir met any barriers
        return !Physics.Raycast(transform.position, rayDir, rayDist, visionMask);
    }


    // Event handler function for when player has entered the sense box
    private void OnTriggerEnter(Collider collider) {
        ITwitchStatus testPlayer = collider.GetComponent<ITwitchStatus>();

        if (testPlayer != null) {
            nearbyTarget = testPlayer;
        }
    }


    // Event handler function for when player has exited the sense box
    private void OnTriggerExit(Collider collider) {
        ITwitchStatus testPlayer = collider.GetComponent<ITwitchStatus>();

        if (testPlayer != null) {
            nearbyTarget = null;
        }
    }

}
