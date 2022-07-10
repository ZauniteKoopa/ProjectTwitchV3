using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoTouching : VirtualSideEffect
{
    private float timePerPoisonTick = 1.5f;


    // Default constructor
    public NoTouching(string description, Specialization s, float tickTime) : base (
        "No Touching",
        description,
        s
    ) {
        timePerPoisonTick = tickTime;
    }


    // Main function to check if this is a player aura side effect (Player)
    public override bool isPlayerAuraEffect() {
        return true;
    }


    // Main function to get the aura rate (if applicable)
    public override float getAuraRate() {
        return timePerPoisonTick;
    }


    // Main function to execute enemy aura with consideration of aura type
    //  Pre: aura != null, vial != null, 0 <= numStacks <= 6
    public override void executeAuraDamage(EnemyAura aura, AuraType auraType, int numStacks, IVial vial) {
        Debug.Assert(aura != null && vial != null && 0 <= numStacks && numStacks <= 6);

        if (auraType == AuraType.NO_TOUCHING) {
            aura.damageAllTargets(0f);
        }
    }
}
