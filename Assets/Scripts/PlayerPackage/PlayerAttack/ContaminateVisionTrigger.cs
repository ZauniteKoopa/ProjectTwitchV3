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
            testEnemy.unitDespawnEvent.AddListener(onTargetDespawn);
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

            // disConnect to all relevant events
            testEnemy.unitDeathEvent.RemoveListener(onTargetDeath);
            testEnemy.unitDespawnEvent.RemoveListener(onTargetDespawn);
            testEnemy.unitPoisonedEvent.RemoveListener(onTargetPoisoned);
            testEnemy.unitCurePoisonEvent.RemoveListener(onTargetCurePoison);
        }
    }


    // Main event handler function for when enemy dies
    private void onTargetDeath(IUnitStatus status) {
        lock (enemyLock) {
            ITwitchUnitStatus enemyStatus = status as ITwitchUnitStatus;
            if (enemyStatus.isPoisoned()) {
                numEnemiesPoisoned--;
            }

            if (numEnemiesPoisoned == 0) {
                enemyPoisonGoneEvent.Invoke();
            }

            enemyStatus.unitDeathEvent.RemoveListener(onTargetDeath);
            enemyStatus.unitDespawnEvent.RemoveListener(onTargetDespawn);
            enemyStatus.unitPoisonedEvent.RemoveListener(onTargetPoisoned);
            enemyStatus.unitCurePoisonEvent.RemoveListener(onTargetCurePoison);
        }
    }


    // Main event handler for when enemy despawns
    //  Very bad, duct tape solution because an enemy only despawns when you die
    private void onTargetDespawn() {
        numEnemiesPoisoned = 0;
        enemyPoisonGoneEvent.Invoke();
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
