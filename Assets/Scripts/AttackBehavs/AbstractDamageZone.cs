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
        initialize();
    }


    // Any additional initialization
    protected virtual void initialize() {}

    
    // On trigger enter, see if you hit ITwitchUnitStatus, if so, add it to targets
    private void OnTriggerEnter(Collider collider) {
        ITwitchUnitStatus colliderTgt = collider.GetComponent<ITwitchUnitStatus>();

        if (colliderTgt != null) {
            lock(targetsLock) {
                if (!inRangeTargets.Contains(colliderTgt)) {
                    inRangeTargets.Add(colliderTgt);
                    colliderTgt.unitDeathEvent.AddListener(onTargetDeath);
                    colliderTgt.unitDespawnEvent.AddListener(delegate{ onTargetDeath(colliderTgt); });
                    unitEnterZone(colliderTgt);
                }
            }
        }
    }


    // On trigger enter, see if you hit ITwitchUnitStatus, if so, add it to targets
    private void OnTriggerExit(Collider collider) {
        ITwitchUnitStatus colliderTgt = collider.GetComponent<ITwitchUnitStatus>();

        if (colliderTgt != null) {
            lock(targetsLock) {
                if (inRangeTargets.Contains(colliderTgt)) {
                    inRangeTargets.Remove(colliderTgt);
                    colliderTgt.unitDeathEvent.RemoveListener(onTargetDeath);
                    colliderTgt.unitDespawnEvent.RemoveListener(delegate{ onTargetDeath(colliderTgt); });
                    unitExitZone(colliderTgt);  
                }
            }
        }
    }


    // Public method to do damage to all enemies within the zone
    //  Pre: dmg is the amount effecting the targets
    //  Post: applies damage to all targets, if at least one of the targets die, trigger targetKilledEvent
    public void damageAllTargets(float dmg) {
        int numTargetsKilled = 0;
        List<ITwitchUnitStatus> damagedTargets = new List<ITwitchUnitStatus>();

        // Go through the original list to make a copy
        lock (targetsLock) {
            foreach (ITwitchUnitStatus target in inRangeTargets) {
                damagedTargets.Add(target);
            }
        }

        // Go through the copy to damage targets
        foreach (ITwitchUnitStatus target in damagedTargets) {
            BossStatus testBoss = target as BossStatus;
            bool prevTransition = (testBoss != null) && testBoss.isPhaseTransitioning();

            damageTarget(target, dmg);
            applyVisualEffects(target);

            // Check if target is alive or boss is transitioning afterwards (cannot be transitioning before this attack)
            bool postTransitioning = (testBoss != null) && testBoss.isPhaseTransitioning();
            if (!target.isAlive() || (postTransitioning && postTransitioning != prevTransition)) {
                numTargetsKilled++;
            }
        }

        // Trigger event if at least 1 enemy is killed
        if (numTargetsKilled > 0) {
            targetKilledEvent.Invoke();
        }
    }


    // Main function to check if target was killed afterwards, mostly used if it's a DELAYED function due to the VFX
    protected bool checkDeath(ITwitchUnitStatus target) {
        BossStatus testBoss = target as BossStatus;
        if (!target.isAlive() || (testBoss != null && testBoss.isPhaseTransitioning())) {
            return true;
        } else {
            return false;
        }
    }


    // Main function to apply timed slow effect to all units within the zone
    //  Pre: speedReduction > 0.0 and duration > 0.0
    //  Post: enemies in the zone on this frame will be slowed by speedFactor for duration seconds
    public void applyTimedSlowEffect(float speedFactor, float duration) {
        // Get the list of slowed units
        List<ITwitchUnitStatus> slowedUnits = new List<ITwitchUnitStatus>();
        foreach(ITwitchUnitStatus unit in inRangeTargets) {
            if (unit != null) {
                slowedUnits.Add(unit);
            }
        }

        // Start sequence
        StartCoroutine(timedSlowEffectSequence(slowedUnits, speedFactor, duration));
    }


    // Main function to slow effect sequence
    private IEnumerator timedSlowEffectSequence(List<ITwitchUnitStatus> slowedUnits, float speedFactor, float duration) {
        foreach (ITwitchUnitStatus unit in slowedUnits) {
            if (unit != null) {
                unit.affectSpeed(speedFactor);
            }
        }

        yield return new WaitForSeconds(duration);

        foreach (ITwitchUnitStatus unit in slowedUnits) {
            if (unit != null) {
                unit.affectSpeed(1f / speedFactor);
            }
        }
    }


    // Main event handler function for when enemy dies
    private void onTargetDeath(IUnitStatus status) {
        ITwitchUnitStatus twitchUnitStatus = status as ITwitchUnitStatus;

        lock (targetsLock) {
            if (twitchUnitStatus != null && inRangeTargets.Contains(twitchUnitStatus)) {
                inRangeTargets.Remove(twitchUnitStatus);
                status.unitDeathEvent.RemoveListener(onTargetDeath);
                twitchUnitStatus.unitDespawnEvent.RemoveListener(delegate{ onTargetDeath(status); });

                unitExitZone(twitchUnitStatus);  
            }
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
    protected abstract void applyVisualEffects(ITwitchUnitStatus tgt);


    // Protected method to check if you can use damage zone
    public abstract bool canUseAbility();


    // Virtual method that can be overriden: what happens when unit enter
    protected virtual void unitEnterZone(ITwitchUnitStatus tgt) {}


    // Virtual method that can be overriden: what happens when unit enter
    protected virtual void unitExitZone(ITwitchUnitStatus tgt) {}

}
