using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enums to indicate vial specializations
public enum Specialization {
    POTENCY,
    POISON,
    REACTIVITY,
    STICKINESS,
    NONE
}

// Enum that considers Aura Requirements
public enum AuraType {
    RADIOACTIVE_EXPUNGE,
    CONTAGION,
    NO_TOUCHING
};


// Enum for ultimate types
public enum UltimateType {
    NONE,
    STEROID
};

// Class that gives a basic side effect that has no effect on stats
public class VirtualSideEffect
{
    // Private instance variables concerning SideEffect information
    private string name;
    private string description;
    private Specialization specialization;


    // Default Constructor: just returns a side effect with no name
    public VirtualSideEffect() {
        name = "?????";
        description = "Doesn't seem ripe yet... But what is it missing!?";
        specialization = Specialization.NONE;
    }


    // Class constructor when name and description changes
    public VirtualSideEffect(string n, string d, Specialization s) {
        name = n;
        description = d;
        specialization = s;
    }


    // Main function to get the name of this SideEffect
    public string getName() {
        return name;
    }


    // Main function to get the description of this side effect
    public string getDescription() {
        return description;
    }


    // Main function to get the specialization of this side effect
    public Specialization getSpecialization() {
        return specialization;
    }


    // Main function to access the bolt damage multiplier of this side effect: can be overriden
    public virtual float boltDamageMultiplier() {
        return 1.0f;
    }


    // Main function to get the decay rate multiplier of this side effect: can be overriden
    public virtual float decayRateMultiplier() {
        return 1.0f;
    }


    // Main function to get the slow rate multiplier: can be overriden
    //  Pre: speedFactor > 0.0f;
    //  Post: speedFactor will be greater than 0.0f
    public virtual float modifyStackSpeedFactor(float speedFactor) {
        return speedFactor;
    }


    // Main function to execute enemy aura with consideration of aura type
    //  Pre: aura != null, vial != null, 0 <= numStacks <= 6
    public virtual void executeAuraDamage(EnemyAura aura, AuraType auraType, int numStacks, IVial vial) {}


    // Main function to check if this is an aura side effect (Enemy)
    //  Pre; none
    public virtual bool isAuraSideEffect() {
        return false;
    }

    // Main function to check if this is a player aura side effect (Player)
    public virtual bool isPlayerAuraEffect() {
        return false;
    }


    // Main function to get the aura rate (if applicable)
    public virtual float getAuraRate() {
        return 0f;
    }


    // Main function to check if this is an ultimate
    //  Pre: none
    //  Post: returns the ultimate type for Twitch's Juice. If return NONE, cannot use this as an ultimate
    public virtual UltimateType getUltType() {
        return UltimateType.NONE;
    }


    // Main function to get ultimate cooldown
    //  Pre: none
    //  Post: cooldown >= 0f
    public virtual float getUltimateCooldown() {
        return 0f;
    }

    // Main function to get ultimate ammo cost
    //  Pre: none
    //  Post: cost >= 0f
    public virtual int getUltimateCost() {
        return 0;
    }


    // Main function to apply steroid to player character
    //  Pre: playerStatus != null
    //  Post: Applies status effect on player
    public virtual void applySteroid(ITwitchStatus player) {}
}
