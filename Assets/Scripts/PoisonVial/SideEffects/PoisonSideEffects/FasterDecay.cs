using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FasterDecay : VirtualSideEffect
{
    [SerializeField]
    private float decayRateReduction = 0.66666666f;


    // Main override function: decay rate
    public override float decayRateMultiplier() {
        return decayRateReduction;
    }


    // Main override function for getting the description
    public override string getDescription() {
        float roundedMultiplier = Mathf.Round((1f / decayRateReduction) * 100f) / 100f;
        return "Poison will affect enemy units at a faster rate! (+ " + roundedMultiplier + " the norm) The time that a unit remains poisoned stays the same";
    }
}
