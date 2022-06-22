using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioactiveExpunge : VirtualSideEffect
{
    private const float EXPUNGE_AURA_DAMAGE = 0.5f;


    // Default constructor
    public RadioactiveExpunge() : base (
        "Radioactive Expunge",
        "Upon contaminating an enemy with 4 or more poison stacks, the enemy will explode, dealing " + EXPUNGE_AURA_DAMAGE + "x the contaminate damage to enemies around them",
        Specialization.REACTIVITY
    ) {}


    // Main function to execute enemy aura with consideration of aura type
    //  Pre: aura != null, vial != null, 0 <= numStacks <= 6
    public override void executeAuraDamage(EnemyAura aura, AuraType auraType, int numStacks, IVial vial) {
        Debug.Assert(aura != null && vial != null && 0 <= numStacks && numStacks <= 6);

        if (auraType == AuraType.RADIOACTIVE_EXPUNGE) {
            float additionalExpungeDamage = vial.getContaminateDamage(numStacks) * EXPUNGE_AURA_DAMAGE;
            aura.damageAllTargets(additionalExpungeDamage);
        }
    }


    // Main function to check if this is an aura side effect
    //  Pre; none
    public override bool isAuraSideEffect() {
        return true;
    }
}
