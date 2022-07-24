using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ImpendingDoom : VirtualSideEffect
{
    [SerializeField]
    private float contaminateDamageMultiplier = 1.5f;
    [SerializeField]
    private float autoContaminateDuration = 3f;

    
    // Main override function for getting the description
    public override string getDescription() {
        return "Upon touching poison, the enemy becomes VOLATILE. Contamination automatically occurs " + autoContaminateDuration + " seconds after hitting enemy, dealing " + contaminateDamageMultiplier + "x more damage and consuming all stacks";
    }


    // Main function to get the contaminate multiplier 
    public override float contaminateMultiplier() {
        return contaminateDamageMultiplier;
    }

    // Main function to check if you made enemy volatile (the auto contaminate status effect on enemies)
    //  Pre: none
    //  Post: returns whether or not this side effect makes you volatile. If it does, this also returns the duration it takes for the auto contaminate to occur
    public override bool makesTargetVolatile(out float autoConDuration) {
        autoConDuration = autoContaminateDuration;
        return true;
    }
}
