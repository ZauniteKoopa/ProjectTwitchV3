using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu]
public class RatTaTatTat : VirtualSideEffect
{
    [SerializeField]
    private float damageReductionPerHit = 0.1f;
    [SerializeField]
    private float minDamageReduction = 0.5f;
    [SerializeField]
    private Transform piercingShot;


    // Main override function for this side effect: uses RNG to calculate if you landed a crit. If so, apply crit damage multiplier
    public override float boltDamageMultiplier(int numUnitsPassed) {
        float curDamageReduction = Mathf.Max(1.0f - (damageReductionPerHit * numUnitsPassed), minDamageReduction);
        return curDamageReduction;
    }


    // Main function to get the overriden basic bolt attack associated with this side effect if it has any
    //  Pre: none
    //  Post: returns a pointer to the prefab's ITwitchBasicAttack
    public override ITwitchBasicAttack getBasicBoltOverride() {
        if (piercingShot == null) {
            Debug.LogError("No piercing shot prefab connected to piercing projectile side effect at all");
        }

        ITwitchBasicAttack boltOverride = piercingShot.GetComponent<ITwitchBasicAttack>();
        if (boltOverride == null) {
            Debug.LogError("No piercing shot override found in the connected prefab that has an ITwitchBasicAttack Component");
        }
        return boltOverride;
    }


    // Main override function for getting the description
    public override string getDescription() {
        float displayedReduction = (damageReductionPerHit * 100f);
        float displayedCap = (1.0f - minDamageReduction) * 100f;
        return "Bolts now become PIERCING, going through enemies. For each enemy hit, the bolt loses " + displayedReduction + "% of the damage, capping at " + displayedCap + "% damage reduction";
    }
}
