using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;


public class ContaminateVisionTrigger : MonoBehaviour
{
    // Main data structure to hold what enemies are present within invisibility vision sensor with lock
    private int numEnemiesPoisoned = 0;
    private readonly object enemyLock = new object();
    
    // Event functions
    public UnityEvent enemyPoisonFoundEvent;
    public UnityEvent enemyPoisonGoneEvent;


    // Upon an enemy entering the trigger, add to list of inRange enemies
    private void OnTriggerEnter(Collider collider) {
        ITwitchUnitStatus testEnemy = collider.GetComponent<ITwitchUnitStatus>();

        if (testEnemy != null) {
            // Add to count if possible
            if (testEnemy.isPoisoned()) {
                lock (enemyLock) {
                    if (numEnemiesPoisoned == 0) {
                        enemyPoisonFoundEvent.Invoke();
                    }

                    numEnemiesPoisoned++;
                }
            }

            // Connect to all relevant events
            testEnemy.unitDeathEvent.AddListener(onTargetDeath);
            testEnemy.unitPoisonedEvent.AddListener(onTargetPoisoned);
            testEnemy.unitCurePoisonEvent.AddListener(onTargetCurePoison);
        }
    }


    // Upon an enemy exiting, remove from list of inRange enemies
    private void OnTriggerExit(Collider collider) {
        ITwitchUnitStatus testEnemy = collider.GetComponent<ITwitchUnitStatus>();

        if (testEnemy != null) {
            // Decrement to count if possible
            if (testEnemy.isPoisoned()) {
                lock (enemyLock) {
                    numEnemiesPoisoned--;

                    if (numEnemiesPoisoned == 0) {
                        enemyPoisonGoneEvent.Invoke();
                    }
                }
            }

            // Connect to all relevant events
            testEnemy.unitDeathEvent.RemoveListener(onTargetDeath);
            testEnemy.unitPoisonedEvent.RemoveListener(onTargetPoisoned);
            testEnemy.unitCurePoisonEvent.RemoveListener(onTargetCurePoison);
        }
    }


    // Main event handler function for when enemy dies
    private void onTargetDeath(IUnitStatus status) {
        lock (enemyLock) {
            numEnemiesPoisoned--;

            if (numEnemiesPoisoned == 0) {
                enemyPoisonGoneEvent.Invoke();
            }
        }
    }


    // Main event handler function when a nearby unit starts getting poisoned
    private void onTargetPoisoned() {
        lock (enemyLock) {
            if (numEnemiesPoisoned == 0) {
                enemyPoisonFoundEvent.Invoke();
            }

            numEnemiesPoisoned++;
        }
    }


    // Main event handler function when a nearby unit starts getting poisoned
    private void onTargetCurePoison() {
        lock (enemyLock) {
            numEnemiesPoisoned--;

            if (numEnemiesPoisoned == 0) {
                enemyPoisonGoneEvent.Invoke();
            }
        }
    }
}
