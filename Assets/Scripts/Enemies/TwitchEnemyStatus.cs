using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// General class for an enemy within the project Twitch game
public class TwitchEnemyStatus : ITwitchUnitStatus
{
    // Instance variables concerning health
    [SerializeField]
    private float maxHealth = 10;
    private float curHealth;
    private readonly object healthLock = new object();

    // Poison management
    private IVial currentPoison = null;
    private int numPoisonStacks = 0;
    private const int MAX_STACKS = 6;
    private readonly object poisonLock = new object();
    private readonly object stacksLock = new object();



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
                Debug.Log("Health: " + curHealth + ", Poison stacks: " + numPoisonStacks);

                // Check death condition
                if (curHealth <= 0.0f) {
                    death();
                }
            }
        }
    }


    // Main method to do poison damage (specific to twitch damage)
    //  initDmg: initial, immediate damage applied to enemy, > 0
    //  poison: PoisonVial that will be inflicted to this enemy.
    //  numStacks: number of stacks applied to enemy when doing immediate damage
    //  Post: damage AND poison will be applied to enemy
    public override void poisonDamage(float initDmg, IVial poison, int numStacks) {
        // Change poison
        lock(poisonLock) {
            currentPoison = poison;
        }

        // Inflict number of stacks
        lock(stacksLock) {
            numPoisonStacks = Mathf.Min(numPoisonStacks + numStacks, MAX_STACKS);
        }

        // Do damage
        damage(initDmg);
    }


    // Main method to do poison damage (specific to twitch damage)
    //  initDmg: initial, immediate damage applied to enemy, > 0
    //  poison: PoisonVial that will be inflicted to this enemy IFF unit isn't already inflicted with poison
    //  numStacks: number of stacks applied to enemy when doing immediate damage
    //  Post: damage AND poison will be applied to enemy
    public override void weakPoisonDamage(float initDmg, IVial poison, int numStacks) {
        // Change poison
        lock(poisonLock) {
            if (currentPoison == null) {
                currentPoison = poison;
            }
        }

        // Inflict number of stacks
        lock(stacksLock) {
            numPoisonStacks = Mathf.Min(numPoisonStacks + numStacks, MAX_STACKS);
        }

        // Do damage
        damage(initDmg);
    }


    // Private helper function to do death sequence
    private void death() {
        unitDeathEvent.Invoke();
        gameObject.SetActive(false);
    }
}
