using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAura : AbstractDamageZone
{
    // Private instacne variables
    private IVial caskPoison;


    // Public method to set poison
    //  Pre: vial != null
    public void setCaskPoison(IVial vial) {
        Debug.Assert(vial != null);
        caskPoison = vial;
        GetComponent<MeshRenderer>().material.color = vial.getColor();
    }


    // Main function to set active
    //  Pre: is a boolean that represent whether to turn this on or off
    //  Post: turns aura on or off
    public void setActive(bool willActive) {
        GetComponent<MeshRenderer>().enabled = willActive;
    }

    // Protected method to override that does damage effect to inRangeTarget
    //  Pre: tgt != null and dmg >= 0
    protected override void damageTarget(ITwitchUnitStatus tgt, float dmg) {
        Debug.Assert(tgt != null && dmg >= 0.0f);

        if (dmg > 0.01f) {
            tgt.damage(dmg, false);
        } else {
            tgt.weakPoisonDamage(dmg, caskPoison, 1);
        }
    }


    // Protected method to apply visual effect to targets
    protected override void applyVisualEffects(ITwitchUnitStatus tgt) {}


    // Protected method to check if you can use damage zone
    public override bool canUseAbility() {
        return true;
    }

    // Virtual method that can be overriden: what happens when unit enter
    protected override void unitEnterZone(ITwitchUnitStatus tgt) {}


    // Virtual method that can be overriden: what happens when unit enter
    protected override void unitExitZone(ITwitchUnitStatus tgt) {}
}
