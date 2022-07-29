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
    ENEMY_TIMED,
    NO_TOUCHING,
    SURPRISE
};


// Enum for ultimate types
public enum UltimateType {
    NONE,
    STEROID,
    LOB
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
    [SerializeField]
    private Sprite icon;


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


    // Main function to get access to this side effect's icon
    public Sprite getIcon() {
        return icon;
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


    // Main function to get the contaminate multiplier 
    public virtual float contaminateMultiplier() {
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


    // Main function to throw ult lobs
    //  Pre: startPosition is the start position of the lob, end position is the end position of the lob, statNum is the important stat value
    //  Post: launches lobbing ultimate
    public virtual void throwLobbingUltimate(Vector3 startPos, Vector3 endPos, int statNum) {}


    // Main boolean to check if you can execute enemies automatically with this vial
    //  Pre: isBoss indicates whether this is a boss or not, 0.0f <= healthPercentRemaining <= 1.0, 0 <= numStacks <= 6
    //  Post: returns a boolean whether or not this enemy can get immediately executed
    public virtual bool canExecute(bool isBoss, float healthPercentRemaining, int numStacks) { return false;}


    // Main function to check if you made enemy volatile (the auto contaminate status effect on enemies)
    //  Pre: none
    //  Post: returns whether or not this side effect makes you volatile. If it does, this also returns the duration it takes for the auto contaminate to occur
    public virtual bool makesTargetVolatile(out float autoContaminateDuration) {
        autoContaminateDuration = 3.0f;
        return false;
    }
}
