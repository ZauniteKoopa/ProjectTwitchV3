using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class IUnitStatus : MonoBehaviour
{
    // Main death event
    public UnityEvent unitDeathEvent;
    

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
}
