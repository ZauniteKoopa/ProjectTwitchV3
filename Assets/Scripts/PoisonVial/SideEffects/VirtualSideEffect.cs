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
    NO_TOUCHING,
    SURPRISE
};


// Enum for ultimate types
public enum UltimateType {
    NONE,
    STEROID
};

// Class that gives a basic side effect that has no effect on stats
[CreateAssetMenu]
public class VirtualSideEffect : ScriptableObject
{
    // Private instance variables concerning SideEffect information
    [SerializeField]
    private string effectName = "?????";
    [SerializeField]
    private Specialization specialization;


    // Main function to get the name of this SideEffect
    public string getName() {
        return effectName;
    }


    // Main function to get the description of this side effect
    public virtual string getDescription() {
        return "Doesn't seem ripe yet... But what is it missing!?";
    }


    // Main function to get the specialization of this side effect
    public Specialization getSpecialization() {
        return specialization;
    }


    // Main function to get the overriden basic bolt attack associated with this side effect if it has any
    //  Pre: none
    //  Post: returns a pointer to the prefab's ITwitchBasicAttack
    public virtual ITwitchBasicAttack getBasicBoltOverride() {
        return null;
    }


    // Main function to access the bolt damage multiplier of this side effect: can be overriden
    //  Pre: the number of units that the bolt went through before hitting this current unit
    public virtual float boltDamageMultiplier(int numUnitsPassed) {
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


    // Main function to execute enemy aura with consideration of aura type. returns true if successful. False if it isn't
    //  Pre: aura != null, vial != null, 0 <= numStacks <= 6, curVialTimer >= 0f (usually its related to a timer)
    //  Post: returns true if you're successful with aura damage, returns false if you aren't successful
    public virtual bool executeAuraDamageTimed(EnemyAura aura, AuraType auraType, int numStacks, IVial vial, float curVialTimer) {return false;}


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
