using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioactiveExpunge : VirtualSideEffect
{
    private float expungeAuraDamage;


    // Default constructor
    public RadioactiveExpunge(string description, Specialization s, float auraDamage) : base (
        "Radioactive Expunge",
        description,
        s
    ) {
        expungeAuraDamage = auraDamage;
    }


    // Main function to execute enemy aura with consideration of aura type
    //  Pre: aura != null, vial != null, 0 <= numStacks <= 6
    public override void executeAuraDamage(EnemyAura aura, AuraType auraType, int numStacks, IVial vial) {
        Debug.Assert(aura != null && vial != null && 0 <= numStacks && numStacks <= 6);

        if (auraType == AuraType.RADIOACTIVE_EXPUNGE) {
            float additionalExpungeDamage = vial.getContaminateDamage(numStacks) * expungeAuraDamage;
            aura.damageAllTargets(additionalExpungeDamage);
        }
    }


    // Main function to check if this is an aura side effect
    //  Pre; none
    public override bool isAuraSideEffect() {
        return true;
    }
}
