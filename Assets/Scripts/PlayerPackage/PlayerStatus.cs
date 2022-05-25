using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatus : IUnitStatus
{
    // Movement speed variables
    [SerializeField]
    private float baseMovementSpeed = 8.0f;
    private float movementSpeedFactor = 1.0f;

    // Main accessor method to get movement speed
    //  Pre: none
    //  Post: returns base movement speed with speed factors applied
    public override float getMovementSpeed() {
        return baseMovementSpeed * movementSpeedFactor;
    }


    // Main method to damage player unit
    //  Pre: damage is a number greater than 0
    //  Post: damage is inflicted on player unit
    public override void damage(float dmg) {
        Debug.Log("Player suffered " + dmg + " damage");
    }
}
