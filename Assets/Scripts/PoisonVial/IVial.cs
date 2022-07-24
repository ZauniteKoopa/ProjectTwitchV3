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
    //  Post: returns how much ammo left
    int getAmmoLeft();


    // Main function to get the basic attack prefab used to clone
    //  Post: returns a pointer to the basic attack prefab's ITwitchBasicAttack
    ITwitchBasicAttack getBoltType();


    // Function to get access to how much immediate damage a bolt / bullet does
    //  Post: returns how much damage a bullet does based on current stats > 0
    float getBoltDamage(int numUnitsPassed);

    
    // Function to get access to poison damage based on the number of stacks a unit has
    //  Pre: 0 < numStacks <= 5
    //  Post: returns the amount of poison damage unit suffers based on this vial's stats > 0
    float getPoisonDamage(int numStacks);


    // Function to get the decay rate of this poison when dealing DoT to enemies
    //  Pre: none
    //  Post: a float that's greater than 0
    float getPoisonDecayRate();


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


    // Function to get access to the base stats of this vial
    //  Pre: none
    //  Post: returns a dictionary with 4 fixed properties: "Poison", "Potency", "Reactivity", and "Stickiness"
    Dictionary<string, int> getStats();


    // Function to upgrade Poison using only one ingredient
    //  Pre: ing != null
    //  Post: Returns whether upgrade is successful. If successful, vial is updated with this ingredient
    bool upgrade(Ingredient ing);


    // Function to upgrade Poison using only two ingredients
    //  Pre: ing1 != null && ing2 != null
    //  Post: Returns whether upgrade is successful. If successful, vial is updated with this ingredient
    bool upgrade(Ingredient ing1, Ingredient ing2);


    // Function to access the color of this vial
    //  Pre: null
    //  Post: Returns a valid color
    Color getColor();


    // Main function to access total stat count
    //  Pre: none
    //  Post: returns the total number of stats. <= than maxStat
    int getCurrentTotalStat();


    // Main function to access max stat count
    //  Pre: none
    //  Post: returns max stat count for this instance
    int getMaxTotalStat();



    // -----------------
    // Side effect specific functions
    // -----------------



    // Main function to get side effect information
    //  Pre: none
    //  Post: returns an array in the following format: [name, description], outputs the side effect's specialization as a separate object
    string[] getSideEffectInfo(out Specialization specialization);


    // Main function to apply Enemy Aura effects
    //  Pre: aura != null and auraType is an enum within VirtualSideEffect that specifies what type of effect you're looking for
    //  Post: If auraType matches side effect, apply the appropriate effects
    void applyEnemyAuraEffects(EnemyAura aura, AuraType auraType, int numStacks);


    // Main function to apply Enemy Aura effects
    //  Pre: aura != null, auraType is an enum within VirtualSideEffect that specifies what type of effect you're looking for, 6 >= numStacks >= 0
    //  Post: If auraType matches side effect, apply the appropriate effects. Returns true if successful, returns false if 
    bool applyEnemyAuraEffectsTimed(EnemyAura aura, AuraType auraType, int numStacks, float auraTimer);


    // If enemy aura can be present. return true;
    //  Pre: 0 <= numStacks <= 6
    //  Post: returns whether the enemy aura can be present
    bool isEnemyAuraPresent(int numStacks);


    // If player aura can be present. return true;
    //  Pre: none
    //  Post: returns whether the player aura can be present
    bool isPlayerAuraPresent();


    // Returns the aura rate of the vial
    //  Pre: none
    //  Post: returns aura rate >= 0.0f
    float getAuraRate();


    // Main function to check if you can actually use the ultimate
    //  Pre: none
    //  Post: returns whether or not you can run ultimate DOESN'T CONSIDER COOLDOWNS
    bool hasUltimate();


    // Returns ultimate cooldown
    //  Pre: none
    //  Post: returns a float >= 0.0f
    float getUltimateCooldown();


    // Returns ultimate cost
    //  Pre: none
    //  Post: returns an int >= 0
    int getUltimateCost();


    // Main function to execute ultimate
    //  Pre: player != null
    //  Post: executes the ultimate listed in side effects, returns true if successful
    bool executeUltimate(ITwitchStatus player, Vector3 dest);


    // Main function to check if you can auto execute the enemy based on health
    //  Pre: isBoss indicates whether this is a boss or not, 0.0f <= healthPercentRemaining <= 1.0, 0 <= numStacks <= 6
    //  Post: returns a boolean whether or not this enemy can get immediately executed
    bool canAutoExecute(bool isBoss, float healthPercentRemaining, int numStacks);


    // Main function to check if you have a side effect
    //  Pre: none
    //  Post: checks if you have a side effect
    bool hasSideEffect();
}
