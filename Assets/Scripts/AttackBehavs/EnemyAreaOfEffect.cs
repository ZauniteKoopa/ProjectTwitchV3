using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAreaOfEffect : AbstractDamageZone
{
    // Main variables to get mesh renderer
    private MeshRenderer meshRender;


    // Initialize variables
    protected override void initialize() {
        meshRender = GetComponent<MeshRenderer>();

        if (meshRender == null) {
            Debug.LogError("No mesh render attached to this object");
        }
    }


    // Protected method to override that does damage effect to inRangeTarget
    protected override void damageTarget(ITwitchUnitStatus tgt, float dmg) {
        tgt.damage(dmg, false);
    }


    // Protected method to apply visual effect to targets
    protected override void applyVisualEffects() {}


    // Protected method to check if you can use damage zone
    public override bool canUseAbility() {
        return true;
    }


    // Main function to change color
    public void changeColor(Color color) {
        if (color == Color.clear) {
            meshRender.enabled = false;
        } else {
            meshRender.enabled = true;
            meshRender.material.color = color;
        }
    }

}
