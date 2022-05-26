using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonVialBolt : AbstractStraightProjectile
{
    // Private instance variables
    private IVial poison;


    // Main method to set poison vial to calculate the damage. If newPoison is null, just use default damage
    public void setVialDamage(IVial newPoison) {
        poison = newPoison;
    }

    // Main method to do damage to this target
    protected override void damageTarget(ITwitchUnitStatus target) {
        target.poisonDamage(poison.getBoltDamage(), poison, 1);
    }

    
    // Main function to handle what happens to projectile body when hitting an enemy (to be abstracted)
    protected override void onTargetHit() {
        Object.Destroy(gameObject);
    }
}
