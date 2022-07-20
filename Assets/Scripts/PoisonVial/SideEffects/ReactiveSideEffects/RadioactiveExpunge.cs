using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class RadioactiveExpunge : VirtualSideEffect
{
    [SerializeField]
    private float expungeAuraDamage = 0.5f;


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

    // Main override function for getting the description
    public override string getDescription() {
        float displayExplodeDamage = expungeAuraDamage * 100f;
        return "Upon contaminating an enemy with 4 or more poison stacks, the enemy will explode, dealing " + displayExplodeDamage + "% the contaminate damage to enemies around them";
    }
}
