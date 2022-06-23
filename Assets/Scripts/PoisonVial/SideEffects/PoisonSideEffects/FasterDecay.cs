using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FasterDecay : VirtualSideEffect
{
    private float decayRateReduction;

    // Main constructor
    public FasterDecay(string description, Specialization s, float reduction) : base(
        "Faster Decay",
        description,
        s
    ) {
        decayRateReduction = reduction;
    }


    // Main override function: decay rate
    public override float decayRateMultiplier() {
        return decayRateReduction;
    }
}
