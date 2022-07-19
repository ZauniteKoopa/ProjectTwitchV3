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
    //  Post: damage AND poison will be applied to enemy
    public abstract void poisonDamage(float initDmg, IVial poison, int numStacks);


    // Main method to do poison damage (specific to twitch damage)
    //  initDmg: initial, immediate damage applied to enemy, > 0
    //  poison: PoisonVial that will be inflicted to this enemy IFF unit isn't already inflicted with poison
    //  numStacks: number of stacks applied to enemy when doing immediate damage
    //  Post: damage AND poison will be applied to enemy
    public abstract void weakPoisonDamage(float initDmg, IVial poison, int numStacks);


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
    }

    

}
