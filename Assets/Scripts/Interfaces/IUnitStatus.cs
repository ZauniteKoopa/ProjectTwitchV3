using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnitDelegate : UnityEvent<IUnitStatus> {};

public abstract class IUnitStatus : MonoBehaviour
{
    // Main death event
    //public UnityEvent unitDeathEvent;
    public UnitDelegate unitDeathEvent;
    

    // Main method to get current movement speed considering all speed status effects on unit
    //  Pre: none
    //  Post: Returns movement speed with speed status effects in mind
    public abstract float getMovementSpeed();


    // Main method to get current base attack for sword swings or ranged attacks 
    //  Pre: none
    //  Post: Returns a float that represents base attack (> 0)
    public abstract float getBaseAttack();


    // Main method to inflict basic damage on unit
    //  Pre: damage is a number greater than 0
    //  Post: unit gets inflicted with damage 
    public abstract bool damage(float dmg);


    // Main function to check if the unit is still alive
    //  Pre: none
    //  Post: returns true is unit is still alive
    public abstract bool isAlive();


    // Main function to reset unit, especially when player dies
    //  Pre: none
    //  Post: If enemy, reset to passive state, not sensing any enemies
    //        If player, reset all cooldowns to 0 and lose collectibles upon death
    public abstract void reset();


    // Main function to slow down or speed up by a specifed speed factor
    //  Pre: speedFactor > 0.0f. If less than 1, slow. Else, fast
    //  Post: speed is affected accordingly
    public abstract void affectSpeed(float speedFactor);
}
