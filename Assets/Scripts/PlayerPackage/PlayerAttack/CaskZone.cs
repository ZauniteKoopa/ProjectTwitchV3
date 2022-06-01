using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CaskZone : AbstractDamageZone
{
    // Private instacne variables
    private IVial caskPoison;


    // Public method to set poison
    //  Pre: vial != null
    public void setCaskPoison(IVial vial) {
        Debug.Assert(vial != null);
        caskPoison = vial;
    }

    // Protected method to override that does damage effect to inRangeTarget
    //  Pre: tgt != null and dmg >= 0
    protected override void damageTarget(ITwitchUnitStatus tgt, float dmg) {
        Debug.Assert(tgt != null && dmg >= 0.0f);
        tgt.weakPoisonDamage(dmg, caskPoison, 1);
    }


    // Protected method to apply visual effect to targets
    protected override void applyVisualEffects() {}


    // Protected method to check if you can use damage zone
    public override bool canUseAbility() {
        return true;
    }
}
