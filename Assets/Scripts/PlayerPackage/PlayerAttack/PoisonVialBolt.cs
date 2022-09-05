using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PoisonVialBolt : AbstractStraightProjectile, ITwitchBasicAttack
{
    // Private instance variables to calculate damage. aim assist for children to adjust aim afterwards
    protected IVial poison;
    private float damageMultiplier = 1.0f;
    [SerializeField]
    private float weakBoltDamage = 1.75f;


    // Main method to set poison vial to calculate the damage. If newPoison is null, just use default damage
    public void setVialDamage(IVial newPoison, float multiplier) {
        Debug.Assert(multiplier > 0.0f);

        poison = newPoison;
        damageMultiplier = multiplier;
        MeshRenderer meshRender = GetComponent<MeshRenderer>();
        
        if (newPoison == null) {
            meshRender.material.color = Color.black;
        } else {
            meshRender.material.color = newPoison.getColor();
        }
    }

    // Main method to do damage to this target
    //  Pre: target != null
    protected override void damageTarget(ITwitchUnitStatus target) {
        Debug.Assert(target != null);

        if (poison != null) {
            target.poisonDamage(getDamage(0), poison, 1, true);
        } else {
            target.damage(getDamage(0), false, true);
        }
    }


    // Main function to get damage the bolt does (private helper function)
    protected float getDamage(int numEnemiesPassed) {
        if (poison == null) {
            return weakBoltDamage * damageMultiplier;
        } else {
            return poison.getBoltDamage(numEnemiesPassed) * damageMultiplier;
        }
    }

    
    // Main function to handle what happens to projectile body when hitting an enemy (to be abstracted)
    protected override void onTargetHit() {
        Object.Destroy(gameObject);
    }


    // Main function to get access to the transform of this component
    //  Pre: none
    //  Post: returns the transform associated with this object
    public Transform getTransform() {
        return transform;
    }
}
