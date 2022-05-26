using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerStatus : ITwitchStatus
{
    // Movement speed variables
    [SerializeField]
    private float baseMovementSpeed = 8.0f;
    private float movementSpeedFactor = 1.0f;

    // Poison vial variables
    private IVial primaryPoisonVial;
    private IVial secondaryPoisonVial;


    //On awake, initialize poison vials (GET RID OF THIS IN CRAFTING)
    private void Awake() {
        primaryPoisonVial = new PoisonVial(3, 0, 2, 0, 40);
        secondaryPoisonVial = new PoisonVial(0, 2, 0, 3, 40);
    }


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


    // Main method to get access to primary poison vial
    //  Pre: none
    //  Post: returns the primary poison vial that player is using, CAN BE NULL
    public override IVial getPrimaryVial() {
        return primaryPoisonVial;
    }


    // Main shorthand method to use primary vial
    //  Pre: ammoCost >= 0
    //  Post: returns if successful. If so, reduces primary vial's ammo
    public override bool usePrimaryVialAmmo(int ammoCost) {
        Debug.Assert(ammoCost >= 0);

        if (primaryPoisonVial == null) {
            return false;
        }

        return primaryPoisonVial.useVial(ammoCost);   
    }


    // Main event handler function for when primary poison vial runs out of ammo
    //  Pre: primary vial ammo <= 0
    private void onPrimaryVialNoAmmo() {
        Debug.Assert(primaryPoisonVial.getAmmoLeft() <= 0);

        primaryPoisonVial = null;
    }


    // Main event handler function for swapping between 2 vials
    //  Pre: none
    //  Post: swaps primary and secondary vials on the fly
    public override void swapVials() {
        IVial tempVial = primaryPoisonVial;
        primaryPoisonVial = secondaryPoisonVial;
        secondaryPoisonVial = tempVial;

        Debug.Log("Vials swapped");
    }
}
