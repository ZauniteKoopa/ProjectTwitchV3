using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyProjectile : AbstractStraightProjectile
{
    // Private instance variables concerning damage
    private float damage;


    // Main function to set up damage
    public void setDamage(float dmg) {
        damage = dmg;
    }


    // Main method to do damage to this target
    protected override void damageTarget(ITwitchUnitStatus target) {
        target.damage(damage, false);
    }

    
    // Main function to handle what happens to projectile body when hitting an enemy
    protected override void onTargetHit() {
        Object.Destroy(gameObject);
    }
}
