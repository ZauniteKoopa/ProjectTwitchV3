using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SprayAndPray : VirtualSideEffect
{
    [SerializeField]
    private float critChance = 30f;
    [SerializeField]
    private float critDamageMultiplier = 2f;


    // Main override function for this side effect: uses RNG to calculate if you landed a crit. If so, apply crit damage multiplier
    public override float boltDamageMultiplier(int numUnitsPassed) {
        float diceRoll = Random.Range(0f, 100f);
        return (diceRoll <= critChance) ? critDamageMultiplier : 1.0f;
    }


    // Main override function for getting the description
    public override string getDescription() {
        return critChance + "% chance to do critical damage with bolts and cask throws! Critical strikes deal " + critDamageMultiplier + "x more damage than usual";
    }
}
