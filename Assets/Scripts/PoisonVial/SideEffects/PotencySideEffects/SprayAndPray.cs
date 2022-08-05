using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu]
public class SprayAndPray : VirtualSideEffect
{
    [SerializeField]
    private float maxCritChance = 30f;
    [SerializeField]
    private float critDamageMultiplier = 2f;

    // Main function to modify damage based on the number of poison stacks an enemy has (usually crit)
    //  Pre: damage > 0 && 0 <= numPoisonStacks <= 6
    //  Post: returns a float that modifies damage if you got crit
    public override float enhanceDamage(float damage, int numPoisonStacks) {
        Debug.Assert(damage >= 0.0f);
        Debug.Assert(numPoisonStacks >= 0 && numPoisonStacks <= 6);

        // roll the dice, if you rolled the dice correctly, get a crit
        float critChance = Mathf.Lerp(0f, maxCritChance, (float)numPoisonStacks / 6f);
        float diceRoll = Random.Range(0f, 100f);
        return (diceRoll <= critChance) ? damage * critDamageMultiplier : damage;
    }


    // Main override function for getting the description
    public override string getDescription() {
        float critPerStack = maxCritChance / 6f;
        critPerStack = Mathf.Round(critPerStack * 10f) / 10f;
        return "Gain " + critPerStack + "% critical chance on bolts for each stack the targeted enemy has, capping at " + maxCritChance + "% chance critical chance at max stacks. Critical bolts do " + critDamageMultiplier + "x more damage than usual";
    }
}
