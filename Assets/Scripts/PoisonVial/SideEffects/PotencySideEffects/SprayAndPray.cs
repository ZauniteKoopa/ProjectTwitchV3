using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayAndPray : VirtualSideEffect
{
    private float critChance;
    private float critDamageMultiplier;

    // Main constructor
    public SprayAndPray(string description, Specialization s, float chance, float multiplier) : base(
        "Spray and Pray",
        description,
        s)
    {
        critChance = chance;
        critDamageMultiplier = multiplier;
    }


    // Main override function for this side effect: uses RNG to calculate if you landed a crit. If so, apply crit damage multiplier
    public override float boltDamageMultiplier() {
        float diceRoll = Random.Range(0f, 100f);
        return (diceRoll <= critChance) ? critDamageMultiplier : 1.0f;
    }
}
