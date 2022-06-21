using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayAndPray : VirtualSideEffect
{
    private const float CRIT_CHANCE = 40f;
    private const float CRIT_DAMAGE_MULTIPLIER = 2.0f;

    // Main constructor
    public SprayAndPray() : base("Spray and Pray", CRIT_CHANCE + "% chance to do critical damage with bolts and cask throws! Critical strikes deal " + CRIT_DAMAGE_MULTIPLIER + "x more damage than usual", Specialization.POTENCY)
    {
        
    }


    // Main override function for this side effect: uses RNG to calculate if you landed a crit. If so, apply crit damage multiplier
    public override float boltDamageMultiplier() {
        float diceRoll = Random.Range(0f, 100f);
        return (diceRoll <= CRIT_CHANCE) ? CRIT_DAMAGE_MULTIPLIER : 1.0f;
    }
}
