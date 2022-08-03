using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ITwitchUnitStatus : IUnitStatus
{
    // Main events for when enemy starts getting poisoned
    public UnityEvent unitPoisonedEvent;
    public UnityEvent unitCurePoisonEvent;
    public UnityEvent unitDespawnEvent;

    // Main method to do poison damage (specific to twitch damage)
    //  initDmg: initial, immediate damage applied to enemy, > 0
    //  poison: PoisonVial that will be inflicted to this enemy.
    //  numStacks: number of stacks applied to enemy when doing immediate damage
    //  canCrit: can the damage given crit
    //  Post: damage AND poison will be applied to enemy
    public abstract void poisonDamage(float initDmg, IVial poison, int numStacks, bool canCrit = false);


    // Main method to do poison damage (specific to twitch damage)
    //  initDmg: initial, immediate damage applied to enemy, > 0
    //  poison: PoisonVial that will be inflicted to this enemy IFF unit isn't already inflicted with poison
    //  numStacks: number of stacks applied to enemy when doing immediate damage
    //  canCrit: can the damage given crit
    //  Post: damage AND poison will be applied to enemy IFF enemy had no poison initially
    public abstract void weakPoisonDamage(float initDmg, IVial poison, int numStacks, bool canCrit = false);


    // Main function to contaminate the unit with the poison they already have
    //  Pre: none
    //  Post: enemy suffers from severe burst damage
    public abstract void contaminate();


    // Main function to check if a unit is poisoned
    //  Pre: none
    //  Post: returns whether or not the unit is poisoned
    public abstract bool isPoisoned();


    // Main function to handle spwning in of the unit (Player enters the game or enemies spawn in the area)
    //  Pre: none
    //  Post: spawns the enemy in IFF not spawned in game yet
    public virtual void spawnIn() {
        reset();
        gameObject.SetActive(true);
    }


    // Main function to handle units despawning from game. (Player disconnects (netcode) or enemy despawns)
    //  Pre: none
    //  Post: spawns the enemy in IFF not spawned in game yet
    public virtual void despawn() {
        unitDespawnEvent.Invoke();
        gameObject.SetActive(false);

        if (statusDisplay != null) {
            statusDisplay.clear();
        }
    }


    // Main function to make this unit manic if they aren't manic already or get rid of manic if they are
    //  MANIC: attack increases by 1.5 its original value BUT armor decreases by 0.5 that value
    //  Pre: 0.0 < manicIntensity < 1.0f;
    public abstract void makeManic(bool willManic, float manicIntensity);

    
    // Main function to get attackChangeFactor
    //  Post: returns the attack multiplier for this unit
    public abstract float getAttackMultiplier();

}
