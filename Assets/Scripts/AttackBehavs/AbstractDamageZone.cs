using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class AbstractDamageZone : MonoBehaviour
{
    // Variables to keep track of collider
    protected HashSet<ITwitchUnitStatus> inRangeTargets;
    protected readonly object targetsLock = new object();

    // Unity Event for when a target has been killed
    public UnityEvent targetKilledEvent;

    
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
    //  Pre: dmg is the amount effecting the targets
    //  Post: applies damage to all targets, if at least one of the targets die, trigger targetKilledEvent
    public void damageAllTargets(float dmg) {
        int numTargetsKilled = 0;

        // Go through each target to apply damage
        foreach (ITwitchUnitStatus target in inRangeTargets) {
            damageTarget(target, dmg);

            if (!target.isAlive()) {
                numTargetsKilled++;
            }
        }

        // Trigger event if at least 1 enemy is killed
        if (numTargetsKilled > 0) {
            targetKilledEvent.Invoke();
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
