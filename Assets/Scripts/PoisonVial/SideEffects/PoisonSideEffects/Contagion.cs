using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Contagion : VirtualSideEffect
{
    [SerializeField]
    private int auraSpreadTime = 2;


    // Main function to execute enemy aura with consideration of aura type. returns true if successful. False if it isn't
    //  Pre: aura != null, vial != null, 0 <= numStacks <= 6, curVialTimer >= 0f (usually its related to a timer)
    //  Post: returns true if you're successful with aura damage, returns false if you aren't successful
    public override bool executeAuraDamageTimed(EnemyAura aura, AuraType auraType, int numStacks, IVial vial, float curVialTimer) {
        Debug.Assert(aura != null && vial != null && 0 <= numStacks && numStacks <= 6 && curVialTimer >= 0f);

        if (auraType == AuraType.ENEMY_TIMED && curVialTimer >= (float)auraSpreadTime) {
            aura.damageAllTargets(0f);
            return true;
        }

        return false;
    }


    // Main function to check if this is an aura side effect
    //  Pre; none
    public override bool isAuraSideEffect() {
        return true;
    }


    // Main override function for getting the description
    public override string getDescription() {
        return "Upon inflcting an enemy with 4 or more poison stacks, enemies will emit a poison fog, infecting those around them with one poison stack every " + auraSpreadTime + " ticks";
    }
}
