using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BoilingBlast : VirtualSideEffect
{
    [SerializeField]
    private float boltMultiplier = 1.25f;
    [SerializeField]
    private Transform shotgunBlast;


    // Main override function for this side effect: uses RNG to calculate if you landed a crit. If so, apply crit damage multiplier
    public override float boltDamageMultiplier(int numUnitsPassed) {
        return boltMultiplier;
    }


    // Main function to get the overriden basic bolt attack associated with this side effect if it has any
    //  Pre: none
    //  Post: returns a pointer to the prefab's ITwitchBasicAttack
    public override ITwitchBasicAttack getBasicBoltOverride() {
        if (shotgunBlast == null) {
            Debug.LogError("No piercing shot prefab connected to piercing projectile side effect at all");
        }

        ITwitchBasicAttack boltOverride = shotgunBlast.GetComponent<ITwitchBasicAttack>();
        if (boltOverride == null) {
            Debug.LogError("No piercing shot override found in the connected prefab that has an ITwitchBasicAttack Component");
        }
        return boltOverride;
    }


    // Main override function for getting the description
    public override string getDescription() {
        return "Bolts become so unstable that they will EXPLODE upon leaving the crossbow, creating a short ranged blast that will do " + boltMultiplier + "x the normal bolt damage";
    }
}
