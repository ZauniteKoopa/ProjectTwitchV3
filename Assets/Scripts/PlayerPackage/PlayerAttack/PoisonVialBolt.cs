using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PoisonVialBolt : AbstractStraightProjectile
{
    // Private instance variables
    private IVial poison;
    [SerializeField]
    private float weakBoltDamage = 1.75f;


    // Main method to set poison vial to calculate the damage. If newPoison is null, just use default damage
    public void setVialDamage(IVial newPoison) {
        poison = newPoison;
    }

    // Main method to do damage to this target
    //  Pre: target != null
    protected override void damageTarget(ITwitchUnitStatus target) {
        Debug.Assert(target != null);

        if (poison != null) {
            target.poisonDamage(poison.getBoltDamage(), poison, 1);
        } else {
            target.damage(weakBoltDamage);
        }
    }

    
    // Main function to handle what happens to projectile body when hitting an enemy (to be abstracted)
    protected override void onTargetHit() {
        Object.Destroy(gameObject);
    }
}
