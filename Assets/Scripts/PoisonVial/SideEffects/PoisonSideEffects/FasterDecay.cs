using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FasterDecay : VirtualSideEffect
{
    private const float DECAY_RATE_REDUCTION = 0.5f;

    // Main constructor
    public FasterDecay() : base(
        "Faster Decay",
        "Poison will effect enemy units at a faster rate! (" + (1f / DECAY_RATE_REDUCTION) +"x the normal rate) The time that a unit remains poisoned stays the same.",
        Specialization.POISON
    ) {}


    // Main override function: decay rate
    public override float decayRateMultiplier() {
        return DECAY_RATE_REDUCTION;
    }
}
