using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InducedParalysis : VirtualSideEffect
{
    private const float PARALYSIS_SLOW_MULTIPLIER = 3.5f;


    // Main constructor
    public InducedParalysis() : base(
        "Induced Paralysis",
        "Poison will passivly slow down enemies more (" + PARALYSIS_SLOW_MULTIPLIER + "x the norm)",
        Specialization.STICKINESS
    ) {}


    // Main function to get the slow rate multiplier: can be overriden
    //  Pre: speedFactor > 0.0f;
    //  Post: speedFactor will be greater than 0.0f
    public override float modifyStackSpeedFactor(float speedFactor) {
        float slowReduction = 1.0f - speedFactor;
        slowReduction *= PARALYSIS_SLOW_MULTIPLIER;

        return 1.0f - slowReduction;
    }
}
