using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVial
{
    // Main function to use vial with given ammo cost
    //  Pre: ammoCost > 0
    //  Post: returns true if successful and decrements ammo accordingly, returns false if not enough ammo
    bool useVial(int ammoCost);


    // Function to get access to how much ammo left for UI displays
    //  Post: returns how much ammo left > 0
    int getAmmoLeft();


    // Function to get access to how much immediate damage a bolt / bullet does
    //  Post: returns how much damage a bullet does based on current stats > 0
    float getBoltDamage();

    
    // Function to get access to poison damage based on the number of stacks a unit has
    //  Pre: 0 < numStacks <= 5
    //  Post: returns the amount of poison damage unit suffers based on this vial's stats > 0
    float getPoisonDamage(int numStacks);


    // Function to calculate how much contaminate burst damage a unit suffers
    //  Pre: 0 < numStacks <= 5
    //  Post: returns burst damage based on number of stacks and vial's stats > 0
    float getContaminateDamage(int numStacks);


    // Function to get cask slowness speed factor
    //  Pre: none
    //  Post: 0 < returnValue <= 1.0
    float getCaskSlowness();


    // Function to calculate how much a unit is slowed by stacked poison
    //  Pre: 0 < numStacks <= 5
    //  Post: 0 < returnValue <= 1.0
    float getStackSlowness(int numStacks);


    // Function to calculate initial cask damage
    //  Pre: none
    //  Post: return value >= 0
    float getInitCaskDamage();
}
