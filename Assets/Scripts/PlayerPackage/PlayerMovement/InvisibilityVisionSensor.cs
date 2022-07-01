using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class InvisibilityVisionSensor : MonoBehaviour
{
    // Main data structure to hold what enemies are present within invisibility vision sensor with lock
    private HashSet<Collider> inRangeEnemies;
    private readonly object enemyLock = new object();
    
    // Reference variables
    private MeshRenderer render;


    // Main function to set reference variables
    private void Awake() {
        render = GetComponent<MeshRenderer>();
        inRangeEnemies = new HashSet<Collider>();
    }


    // Upon an enemy entering the trigger, add to list of inRange enemies
    private void OnTriggerEnter(Collider collider) {
        ITwitchUnitStatus testEnemy = collider.GetComponent<ITwitchUnitStatus>();

        lock (enemyLock) {
            if (testEnemy != null && !inRangeEnemies.Contains(collider)) {
                inRangeEnemies.Add(collider);
                testEnemy.unitDeathEvent.AddListener(onTargetDeath);
            }
        }
    }


    // Upon an enemy exiting, remove from list of inRange enemies
    private void OnTriggerExit(Collider collider) {
        ITwitchUnitStatus testEnemy = collider.GetComponent<ITwitchUnitStatus>();

        lock (enemyLock) {
            if (testEnemy != null && inRangeEnemies.Contains(collider)) {
                inRangeEnemies.Remove(collider);
                testEnemy.unitDeathEvent.RemoveListener(onTargetDeath);
            }
        }
    }


    // Main event handler function for when enemy dies
    private void onTargetDeath(IUnitStatus status) {
        Collider unitCollider = status.GetComponent<Collider>();

        lock (enemyLock) {
            if (unitCollider != null && inRangeEnemies.Contains(unitCollider)) {
                inRangeEnemies.Remove(unitCollider);
                status.unitDeathEvent.RemoveListener(onTargetDeath);
            }
        }
    }


    // Public function to see if enemy is within invisibility range
    //  Pre: collider != null
    //  Post: returns true if enemy is within range
    public bool isInInvisibilityRange(Collider tgt) {
        Debug.Assert(tgt != null);
        bool inRange;

        // Put a lock to avoid synch issues
        lock (enemyLock) {
            inRange = inRangeEnemies.Contains(tgt);
        }

        return inRange;
    }


    // Public function to make this visible
    //  Pre: visible is whether or not you want to make this sensor visible to the player
    //  Post: changes visibility of mesh depending on visible variable
    public void makeVisible(bool visible) {
        render.enabled = visible;
    }
}
