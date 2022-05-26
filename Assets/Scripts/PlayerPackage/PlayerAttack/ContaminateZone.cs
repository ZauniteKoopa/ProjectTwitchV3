using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContaminateZone : AbstractDamageZone
{
    // Protected method to override that does damage effect to inRangeTarget
    protected override void damageTarget(ITwitchUnitStatus tgt, float dmg) {
        tgt.contaminate();
    }


    // Protected method to apply visual effect to targets
    protected override void applyVisualEffects() {}
}
