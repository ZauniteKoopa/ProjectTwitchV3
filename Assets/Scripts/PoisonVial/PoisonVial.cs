using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PoisonVial : IVial
{
    // Main stats
    private int potency;
    private int poison;
    private int reactivity;
    private int stickiness;

    // Ammo management
    private int ammo;
    private const int MAX_AMMO = 60;

    //Constants for bullet damage (find a wat to make this editable from the designer)
    private const float BASE_DAMAGE = 2f;
    private const float DMG_GROWTH = 0.65f;

    //constants for poison stack damage
    private const float BASE_POISON = 0f;
    private const float POISON_GROWTH = 0.15f;

    //constants for contaminate damage
    private const float BASE_CONTAMINATE_DMG = 3f;
    private const float BASE_CON_GROWTH = 0.5f;
    private const float BASE_STACK_DMG = 1f;
    private const float STACK_DMG_GROWTH = 0.25f;

    //constants for stack slowness
    private const float BASE_SLOWNESS = 0.9f;
    private const float SLOWNESS_GROWTH = -0.025f;

    // Constants for cask slowness
    private const float BASE_CASK_SLOWNESS = 0.7f;
    private const float CASK_SLOWNESS_GROWTH = -0.05f;



    // Main constructor for a poison vial
    //  Pre: initialAmmo > 0 and 0 <= all stats <= 5
    public PoisonVial(int pot, int poi, int r, int s, int initialAmmo) {
        Debug.Assert(initialAmmo > 0);
        Debug.Assert(pot >= 0 && pot <= 5);
        Debug.Assert(poi >= 0 && poi <= 5);
        Debug.Assert(r >= 0 && r <= 5);
        Debug.Assert(s >= 0 && s <= 5);

        potency = pot;
        poison = poi;
        reactivity = r;
        stickiness = s;
        ammo = initialAmmo;
    }


    // Main function to use vial with given ammo cost
    //  Pre: ammoCost > 0
    //  Post: returns true if successful and decrements ammo accordingly, returns false if not enough ammo.
    public bool useVial(int ammoCost) {
        Debug.Assert(ammoCost >= 0);

        if (ammoCost > ammo) {
            return false;
        }

        ammo -= ammoCost;
        return true;
    }


    // Function to get access to how much ammo left for UI displays
    //  Post: returns how much ammo left. Can be negative
    public int getAmmoLeft() {   
        return ammo;
    }


    // Function to get access to the max vial size (It's a constant)
    public int getMaxVialSize() {
        Debug.Assert(MAX_AMMO >= 0);
        return MAX_AMMO;
    }


    // Function to get access to how much immediate damage a bolt / bullet does
    //  Post: returns how much damage a bullet does based on current stats > 0
    public float getBoltDamage() {
        float boltDamage = BASE_DAMAGE + (DMG_GROWTH * potency);

        Debug.Assert(boltDamage >= 0.0f);
        return boltDamage;
    }

    
    // Function to get access to poison damage based on the number of stacks a unit has
    //  Pre: 0 < numStacks <= 5
    //  Post: returns the amount of poison damage unit suffers based on this vial's stats > 0
    public float getPoisonDamage(int numStacks) {
        Debug.Assert(numStacks > 0 && numStacks <= 6);

        float poisonDamage = (BASE_POISON + (POISON_GROWTH * poison)) * numStacks;

        Debug.Assert(poisonDamage >= 0.0f);
        return poisonDamage;
    }


    // Function to calculate how much contaminate burst damage a unit suffers
    //  Pre: 0 < numStacks <= 5
    //  Post: returns burst damage based on number of stacks and vial's stats > 0
    public float getContaminateDamage(int numStacks) {
        Debug.Assert(numStacks > 0 && numStacks <= 6);

        float curBaseDmg = BASE_CONTAMINATE_DMG + (BASE_CON_GROWTH * reactivity);
        float stackDmg = BASE_STACK_DMG + (STACK_DMG_GROWTH * reactivity);
        float contaminateDamage = curBaseDmg + (stackDmg * numStacks);
        
        Debug.Assert(contaminateDamage >= 0.0f);
        return contaminateDamage;
    }


    // Function to get cask slowness speed factor
    //  Pre: none
    //  Post: 0 <= returnValue <= 1.0
    public float getCaskSlowness() {
        float caskSlowness = BASE_CASK_SLOWNESS + (CASK_SLOWNESS_GROWTH * stickiness);

        Debug.Assert(caskSlowness >= 0.0f && caskSlowness <= 1.0f);
        return caskSlowness;
    }


    // Function to calculate how much a unit is slowed by stacked poison
    //  Pre: 0 < numStacks <= 5
    //  Post: 0 < returnValue <= 1.0
    public float getStackSlowness(int numStacks) {
        Debug.Assert(numStacks > 0 && numStacks <= 6);

        float stackSlowness = (stickiness > 0) ? BASE_SLOWNESS + (SLOWNESS_GROWTH * (stickiness - 1)) : 1.0f;

        Debug.Assert(stackSlowness > 0.0f && stackSlowness <= 1.0f);
        return stackSlowness;
    }


    // Function to calculate initial cask damage
    //  Pre: none
    //  Post: return value >= 0
    public float getInitCaskDamage() {
        return 2f * getBoltDamage();
    }


    // Function to get access to the base stats of this vial
    //  Pre: none
    //  Post: returns a dictionary with 4 fixed properties: "Poison", "Potency", "Reactivity", and "Stickiness"
    public Dictionary<string, int> getStats() {
        Dictionary<string, int> statDict = new Dictionary<string, int>();

        statDict.Add("Potency", potency);
        statDict.Add("Poison", poison);
        statDict.Add("Reactivity", reactivity);
        statDict.Add("Stickiness", stickiness);

        Debug.Assert(statDict.ContainsKey("Potency") && statDict.ContainsKey("Poison") && statDict.ContainsKey("Reactivity") && statDict.ContainsKey("Stickiness"));
        return statDict;
    }
}
