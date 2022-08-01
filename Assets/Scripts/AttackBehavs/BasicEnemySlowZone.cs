using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemySlowZone : AbstractDamageZone
{
    private float slowEffect = 0.75f;
    private float slowDuration = 3.5f;


    // Main function to set up slow zone
    //  Pre: effect > 0.0f && duration > 0.0f
    //  Post: set up slow zone with current variables
    public void setUpSlowZone(float effect, float duration) {
        slowEffect = effect;
        slowDuration = duration;
        StartCoroutine(slowZoneSequence());
    }


    // Main Coroutine for slow zone function
    private IEnumerator slowZoneSequence() {
        yield return new WaitForSeconds(slowDuration);

        // Banish to the shadow world (Please change me)
        yield return destroyZoneSequence();
    }


    // Main function to destroy slow zone
    public void destroyZone() {
        StartCoroutine(destroyZoneSequence());
    }

    // Main coroutine for destroying zone
    private IEnumerator destroyZoneSequence() {
        transform.position = new Vector3(0f, 10000000000000000f, 0f);
        yield return new WaitForSeconds(0.1f);
        Object.Destroy(gameObject);
    }


    // Protected method to override that does damage effect to inRangeTarget
    protected override void damageTarget(ITwitchUnitStatus tgt, float dmg) {}


    // Protected method to apply visual effect to targets
    protected override void applyVisualEffects(ITwitchUnitStatus tgt) {}


    // Protected method to check if you can use damage zone
    public override bool canUseAbility() {return false;}


    // Virtual method that can be overriden: what happens when unit enter
    protected override void unitEnterZone(ITwitchUnitStatus tgt) {
        tgt.affectSpeed(slowEffect);
    }


    // Virtual method that can be overriden: what happens when unit enter
    protected override void unitExitZone(ITwitchUnitStatus tgt) {
        tgt.affectSpeed(1f / slowEffect);
    }
}
