using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSensing : MonoBehaviour
{
    // Variables for sensing
    private ITwitchStatus nearbyTarget;
    private bool playerSpotted = false;

    // Main variables for short term memory
    [SerializeField]
    private float forgetDuration = 1f;
    private Coroutine forgetRoutine;

    // Main method to invoke events onto brain
    [SerializeField]
    private IEnemyBehavior brain;
    private Collider body;


    // On awake, error check and get body
    private void Awake() {
        if (forgetDuration < 0f) {
            Debug.LogError("Forget duration for enemy sensor is negative. Should be zero or positive", transform);
        }

        if (brain == null) {
            Debug.LogError("Enemy Sensor is not connected to a behavior tree or AI behavior handler", transform);
        }

        body = brain.GetComponent<Collider>();
        if (body == null) {
            Debug.LogError("Enemy sensor not connected to a collider to check with. Ensure that a collider is connected to the part with the brain", brain.transform);
        }

        nearbyTarget = FindObjectOfType<ITwitchStatus>();
    }


    // On each fixed update frame, 
    private void FixedUpdate() {
        bool seePlayer = nearbyTarget.isVisible(body);

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
}
