using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TouchSensor : IBlockerSensor
{
    // Variables to update number of walls touching
    private int numWallsTouched = 0;
    private readonly object numWallsLock = new object();

    // Main function to check sensor
    //  returns true is numWallsTocuhed > 0
    public override bool isBlocked() {
        bool blocked = false;

        lock(numWallsLock) {
            blocked = numWallsTouched > 0;
        }

        return blocked;
    }


    // Main event handler function to collect colliders as they enter the trigger box
    //  Pre: collision layers must be set because this considers all collisions possible
    //  Post: Increments numWallsTouched
    private void OnTriggerEnter(Collider collider) {
        lock(numWallsLock) {
            numWallsTouched++;
        }

        // Hittable object
        IHittable hittableObj = collider.GetComponent<IHittable>();
        if (hittableObj != null) {
            hittableObj.destroyedEvent.AddListener(onHittableDestroyed);
        }
    }

    // Main event handler function to remove colliders when they exit the trigger box
    //  Pre: collision layers must be set because this considers all collisions possible
    //  Post: Decrements numWallsTouched
    private void OnTriggerExit(Collider collider) {
        lock(numWallsLock) {
            numWallsTouched -= (numWallsTouched == 0) ? 0 : 1;
        }

        // Hittable object
        IHittable hittableObj = collider.GetComponent<IHittable>();
        if (hittableObj != null) {
            hittableObj.destroyedEvent.RemoveListener(onHittableDestroyed);
        }
    }


    //
    //
    //
    private void onHittableDestroyed(IHittable destroyedObj) {
        lock(numWallsLock) {
            numWallsTouched -= (numWallsTouched == 0) ? 0 : 1;
        }

        destroyedObj.destroyedEvent.RemoveListener(onHittableDestroyed);
    }
}
