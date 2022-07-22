using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PoisonVialBolt : AbstractStraightProjectile, ITwitchBasicAttack
{
    // Private instance variables
    protected IVial poison;
    [SerializeField]
    private float weakBoltDamage = 1.75f;


    // Main method to set poison vial to calculate the damage. If newPoison is null, just use default damage
    public void setVialDamage(IVial newPoison) {
        poison = newPoison;
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
            target.poisonDamage(poison.getBoltDamage(0), poison, 1);
        } else {
            target.damage(weakBoltDamage, false);
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
