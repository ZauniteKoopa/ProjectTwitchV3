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

    // Poison management damage
    private IVial currentPoison = null;
    private int numPoisonStacks = 0;
    private readonly object poisonLock = new object();

    // Poison timer
    private int curTick = 0;
    private const float TIME_PER_TICK = 1.0f;
    private const int MAX_STACKS = 6;
    private const int MAX_TICKS = 6;
    private Coroutine poisonDotRoutine = null;



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


    // Main function to handle poison loop
    //  Pre: curTick <= MAX_TICKS
    //  Post: does damage over time using IEnumerators
    private IEnumerator poisonDotLoop() {
        Debug.Assert(curTick <= MAX_TICKS);
        
        // Wait for first poison tick
        yield return new WaitForSeconds(TIME_PER_TICK);

        // Main tick loop
        bool onLastTick = false;
        while (currentPoison != null && !onLastTick) {

            // Inflict poison damage and update tick status
            float poisonTickDamage;
            lock(poisonLock) {
                poisonTickDamage = currentPoison.getPoisonDamage(numPoisonStacks);
                curTick++;
                onLastTick = curTick == MAX_TICKS;
            }

            damage(poisonTickDamage);

            // Wait for the next tick
            yield return new WaitForSeconds(TIME_PER_TICK);
        }

        // At the end of the last tick, get rid of poison
        lock (poisonLock) {
            curTick = 0;
            numPoisonStacks = 0;
            currentPoison = null;
            poisonDotRoutine = null;
        }

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
    //  initDmg: initial, immediate damage applied to enemy, >= 0
    //  poison: PoisonVial that will be inflicted to this enemy. Must not be null
    //  numStacks: number of stacks applied to enemy when doing immediate damage >= 0
    //  Post: damage AND poison will be applied to enemy
    public override void poisonDamage(float initDmg, IVial poison, int numStacks) {
        Debug.Assert(initDmg >= 0.0f && numStacks >= 0);
        Debug.Assert(poison != null);

        // Change poison and start poison DoT loop if it hadn't started already
        lock(poisonLock) {
            // Edit poison variables
            currentPoison = poison;
            curTick = 0;
            numPoisonStacks = Mathf.Min(numPoisonStacks + numStacks, MAX_STACKS);

            // Run poison sequence if none are running at this point
            if (poisonDotRoutine == null) {
                poisonDotRoutine = StartCoroutine(poisonDotLoop());
            }
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

        // Edit poison variables
        lock(poisonLock) {
            // Change poison
            if (currentPoison == null) {
                currentPoison = poison;
            }

            // Update poisonDoT Sequence if updated
            if (poisonDotRoutine == null) {
                poisonDotRoutine = StartCoroutine(poisonDotLoop());
            }

            // Change poison stacks
            curTick = 0;
            numPoisonStacks = Mathf.Min(numPoisonStacks + numStacks, MAX_STACKS);
        }

        // Do damage
        damage(initDmg);
    }


    // Main function to contaminate the unit with the poison they already have
    //  Pre: none
    //  Post: enemy suffers from severe burst damage
    public override void contaminate() {
        // Get necessary information to avoid synch errors
        IVial tempVial = null;
        int tempStacks = 0;

        lock (poisonLock) {
            tempVial = currentPoison;
            tempStacks = numPoisonStacks;
        }

        // Apply damage
        if (tempVial != null) {
            damage(tempVial.getContaminateDamage(tempStacks));
        }
    }


    // Private helper function to do death sequence
    private void death() {
        unitDeathEvent.Invoke();
        gameObject.SetActive(false);
    }
}
