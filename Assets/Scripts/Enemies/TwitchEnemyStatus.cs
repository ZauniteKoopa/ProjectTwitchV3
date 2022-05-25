using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// General class for an enemy within the project Twitch game
public class TwitchEnemyStatus : IUnitStatus
{
    // Instance variables concerning health
    [SerializeField]
    private float maxHealth = 10;
    private float curHealth;
    private readonly object healthLock = new object();


    // On awake, set up variables
    private void Awake() {
        curHealth = maxHealth;
    }

    // Main method to get current movement speed considering all speed status effects on unit
    //  Pre: none
    //  Post: Returns movement speed with speed status effects in mind
    public override float getMovementSpeed() {
        return 0.0f;
    }


    // Main method to inflict basic damage on unit
    //  Pre: damage is a number greater than 0
    //  Post: unit gets inflicted with damage 
    public override void damage(float dmg) {
        // Apply damage. Use a lock to make sure changes to health are synchronized
        lock(healthLock) {

            // Only change health is still alive. No need to kill unit more than once
            if (curHealth > 0.0f){
                curHealth -= dmg;
                Debug.Log(curHealth);

                // Check death condition
                if (curHealth <= 0.0f) {
                    death();
                }
            }
        }
    }


    // Private helper function to do death sequence
    private void death() {
        unitDeathEvent.Invoke();
        gameObject.SetActive(false);
    }
}
