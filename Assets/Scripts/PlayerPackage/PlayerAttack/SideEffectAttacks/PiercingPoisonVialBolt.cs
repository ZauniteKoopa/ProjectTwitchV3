using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PiercingPoisonVialBolt : PoisonVialBolt
{
    // Keep track of number of enemies hit
    private int numEnemiesHit = 0;

    // Main method to do damage to this target
    //  Pre: target != null
    protected override void damageTarget(ITwitchUnitStatus target) {
        Debug.Assert(target != null);

        if (poison != null) {
            target.poisonDamage(getDamage(numEnemiesHit), poison, 1);
        } else {
            base.damageTarget(target);
        }
    }

    
    // Main function to handle what happens to projectile body when hitting an enemy (to be abstracted)
    protected override void onTargetHit() {
        numEnemiesHit++;
    }
}
