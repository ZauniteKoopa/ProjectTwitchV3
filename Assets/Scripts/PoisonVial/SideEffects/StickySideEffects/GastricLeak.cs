using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GastricLeak : VirtualSideEffect
{
    [SerializeField]
    private float leakInterval = 3.5f;
    [SerializeField]
    private float leakSlow = 0.75f;
    [SerializeField]
    private BasicEnemySlowZone leakPuddle;
    
    // Main function to execute enemy aura with consideration of aura type. returns true if successful. False if it isn't
    //  Pre: aura != null, vial != null, 0 <= numStacks <= 6, curVialTimer >= 0f (usually its related to a timer)
    //  Post: returns true if you're successful with aura damage, returns false if you aren't successful
    public override bool executeAuraDamageTimed(EnemyAura aura, AuraType auraType, int numStacks, IVial vial, float curVialTimer) {
        Debug.Assert(aura != null && vial != null && 0 <= numStacks && numStacks <= 6 && curVialTimer >= 0f);

        if (auraType == AuraType.ENEMY_TIMED && curVialTimer >= (float)leakInterval) {
            BasicEnemySlowZone currentPuddle = Object.Instantiate(leakPuddle, aura.transform.position, Quaternion.identity);
            currentPuddle.setUpSlowZone(leakSlow, leakInterval);
            return true;
        }

        return false;
    }


    // Main function to check if this is an aura side effect
    //  Pre; none
    public override bool isAuraSideEffect() {
        return true;
    }


    // Main override function for getting the description
    public override string getDescription() {
        float leakSlowPercent = leakSlow * 100f;
        return "Upon inflcting an enemy with 4 or more poison stacks, enemies will leak out a slowing puddle every " + leakInterval + " seconds that slows down nearby enemies by " + leakSlow + "%. Enemies can only leak 1 puddle at a time";
    }
}
