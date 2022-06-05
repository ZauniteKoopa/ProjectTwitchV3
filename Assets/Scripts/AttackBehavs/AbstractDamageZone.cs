using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDamageZone : MonoBehaviour
{
    // Variables to keep track of collider
    protected HashSet<ITwitchUnitStatus> inRangeTargets;
    protected readonly object targetsLock = new object();

    
    // On awake, initialize
    private void Awake() {
        inRangeTargets = new HashSet<ITwitchUnitStatus>();
    }

    
    // On trigger enter, see if you hit ITwitchUnitStatus, if so, add it to targets
    private void OnTriggerEnter(Collider collider) {
        ITwitchUnitStatus colliderTgt = collider.GetComponent<ITwitchUnitStatus>();

        if (colliderTgt != null) {
            lock(targetsLock) {
                inRangeTargets.Add(colliderTgt);
                unitEnterZone(colliderTgt);
            }
        }
    }


    // On trigger enter, see if you hit ITwitchUnitStatus, if so, add it to targets
    private void OnTriggerExit(Collider collider) {
        ITwitchUnitStatus colliderTgt = collider.GetComponent<ITwitchUnitStatus>();

        if (colliderTgt != null) {
            lock(targetsLock) {
                inRangeTargets.Remove(colliderTgt);
                unitExitZone(colliderTgt);
            }
        }
    }


    // Public method to do damage to all enemies within the zone
    public void damageAllTargets(float dmg) {
        foreach (ITwitchUnitStatus target in inRangeTargets) {
            damageTarget(target, dmg);
        }
    }


    // When object is destroyed, free all units
    private void OnDestroy() {
        lock (targetsLock) {
            foreach (ITwitchUnitStatus target in inRangeTargets) {
                unitExitZone(target);
            }
        }
    }


    // Protected method to override that does damage effect to inRangeTarget
    protected abstract void damageTarget(ITwitchUnitStatus tgt, float dmg);


    // Protected method to apply visual effect to targets
    protected abstract void applyVisualEffects();


    // Protected method to check if you can use damage zone
    public abstract bool canUseAbility();


    // Virtual method that can be overriden: what happens when unit enter
    protected virtual void unitEnterZone(ITwitchUnitStatus tgt) {}


    // Virtual method that can be overriden: what happens when unit enter
    protected virtual void unitExitZone(ITwitchUnitStatus tgt) {}
}
