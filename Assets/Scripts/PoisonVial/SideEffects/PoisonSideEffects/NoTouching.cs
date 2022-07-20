using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class NoTouching : VirtualSideEffect
{
    [SerializeField]
    private float timePerPoisonTick = 1.5f;


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


    // Main override function for getting the description
    public override string getDescription() {
        return "The scent of the poison becomes so viral that when enemies are in proximity to Twitch, they get 1 stack per " + timePerPoisonTick + " seconds";
    }
}
