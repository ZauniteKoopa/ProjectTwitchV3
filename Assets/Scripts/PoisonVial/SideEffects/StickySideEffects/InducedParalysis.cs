using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InducedParalysis : VirtualSideEffect
{
    private float paralysisSlowMultiplier = 3.5f;


    // Main constructor
    public InducedParalysis(string description, Specialization s, float slowMultiplier) : base(
        "Induced Paralysis",
        description,
        s
    ) {
        paralysisSlowMultiplier = slowMultiplier;
    }


    // Main function to get the slow rate multiplier: can be overriden
    //  Pre: speedFactor > 0.0f;
    //  Post: speedFactor will be greater than 0.0f
    public override float modifyStackSpeedFactor(float speedFactor) {
        float slowReduction = 1.0f - speedFactor;
        slowReduction *= paralysisSlowMultiplier;

        return 1.0f - slowReduction;
    }
}
