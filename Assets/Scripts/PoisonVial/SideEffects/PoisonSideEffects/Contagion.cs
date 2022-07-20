using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Contagion : VirtualSideEffect
{
    [SerializeField]
    private float auraSpreadTime = 2f;


    // Main function to execute enemy aura with consideration of aura type
    //  Pre: aura != null, vial != null, 0 <= numStacks <= 6
    public override void executeAuraDamage(EnemyAura aura, AuraType auraType, int numStacks, IVial vial) {
        Debug.Assert(aura != null && vial != null && 0 <= numStacks && numStacks <= 6);

        if (auraType == AuraType.CONTAGION) {
            aura.damageAllTargets(0f);
        }
    }


    // Main function to check if this is an aura side effect
    //  Pre; none
    public override bool isAuraSideEffect() {
        return true;
    }


    // Main override function for getting the description
    public override string getDescription() {
        return "Upon inflcting an enemy with 4 or more poison stacks, enemies will emit a poison fog, infecting those around them with one poison stack every " + auraSpreadTime + " seconds";
    }
}
