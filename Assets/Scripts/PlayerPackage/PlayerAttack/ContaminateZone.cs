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


    // Protected method to check if you can use damage zone
    //  Returns true IFF at least 1 POISONED enemy in range
    public override bool canUseAbility() {
        bool canUse = false;

        lock(targetsLock) {
            foreach(ITwitchUnitStatus tgt in inRangeTargets) {
                canUse = tgt.isPoisoned();

                if (canUse) {
                    break;
                }
            }
        }

        return canUse;
    }
}
