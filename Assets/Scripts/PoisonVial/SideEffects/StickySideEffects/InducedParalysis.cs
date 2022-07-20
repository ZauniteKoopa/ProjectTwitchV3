using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class InducedParalysis : VirtualSideEffect
{
    [SerializeField]
    private float paralysisSlowMultiplier = 4f;


    // Main function to get the slow rate multiplier: can be overriden
    //  Pre: speedFactor > 0.0f;
    //  Post: speedFactor will be greater than 0.0f
    public override float modifyStackSpeedFactor(float speedFactor) {
        float slowReduction = 1.0f - speedFactor;
        slowReduction *= paralysisSlowMultiplier;

        return 1.0f - slowReduction;
    }


    // Main override function for getting the description
    public override string getDescription() {
        return "Poison will passively slow down enemies more (" + paralysisSlowMultiplier + "x the norm)";
    }
}
