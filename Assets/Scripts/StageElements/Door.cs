using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    private Vector3 closedPosition;
    private bool setup = false;
    private Coroutine openingRoutine = null;


    // On awake, set everything up
    private void Awake() {
        if (!setup) {
            closedPosition = transform.position;
            setup = true;
        }
    }


    // Main function to open the door
    //  Pre: none
    //  Post: opens the door (door disappears)
    public void open() {
        // Only deactivate door if not active in hierarchy
        if (gameObject.activeInHierarchy) {
            openingRoutine = StartCoroutine(openSequence());
        }
    }


    // Main function to close the door
    //  Pre: none
    //  Post: closes the door
    public void close() {
        // If there's an opening routine going on, stop the routine
        if (openingRoutine != null) {
            StopCoroutine(openingRoutine);
            openingRoutine = null;
        }

        // Set it up if the door was previously inactive
        if (!setup) {
            closedPosition = transform.position;
            setup = true;
        }

        transform.position = closedPosition;
        gameObject.SetActive(true);
    }


    // Opening coroutine because ObjectDeactivation doesn't trigger OnTriigerExit bleagh
    private IEnumerator openSequence() {
        // banish to the shadow realm
        transform.position = 10000000000f * Vector3.up;

        // Wait for 2 frames for collisions to update
        for (int i = 0; i < 2; i++) {
            yield return new WaitForFixedUpdate();
        }

        // deactivate
        gameObject.SetActive(false);
        openingRoutine = null;
    }
}
