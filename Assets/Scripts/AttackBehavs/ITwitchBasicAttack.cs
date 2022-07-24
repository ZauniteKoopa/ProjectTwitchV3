using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITwitchBasicAttack
{
    // Main function to connect basic attack damage to this new poison
    //  Pre: newPoison CAN be null or non-null
    //  Post: poison vial is now connected to this attack to calculate damage. Damage calculations depend on the basic attack instance
    void setVialDamage(IVial newPoison);


    // Main function to fire the basic attack towards a certain direction
    //  Pre: projDir is the direction that the projectile is facing and projSpeed is how fast the projectile goes
    //  Post: sets up basic attack to go a certain direction
    void setUpMovement(Vector3 projDir, float projSpeed);


    // Main function to get access to the transform of this component
    //  Pre: none
    //  Post: returns the transform associated with this object
    Transform getTransform();
}
