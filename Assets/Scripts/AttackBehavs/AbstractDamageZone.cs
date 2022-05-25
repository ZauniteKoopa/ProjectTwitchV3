using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractDamageZone : MonoBehaviour
{
    // Variables to keep track of collider
    protected HashSet<Hurtbox> inRangeTargets;
    protected readonly object targetsLock = new object();

    
    // On awake, initialize
    private void Awake() {
        inRangeTargets = new HashSet<Hurtbox>();
    }

    
    // On trigger enter, see if you hit hurtbox, if so, add it to targets
    private void OnTriggerEnter(Collider collider) {
        Hurtbox colliderHurtbox = collider.GetComponent<Hurtbox>();

        if (colliderHurtbox != null) {
            lock(targetsLock) {
                inRangeTargets.Add(colliderHurtbox);
            }
        }
    }


    // On trigger enter, see if you hit hurtbox, if so, add it to targets
    private void OnTriggerExit(Collider collider) {
        Hurtbox colliderHurtbox = collider.GetComponent<Hurtbox>();

        if (colliderHurtbox != null) {
            lock(targetsLock) {
                inRangeTargets.Remove(colliderHurtbox);
            }
        }
    }


    // Public method to do damage to all enemies within the zone
    public void damageAllTargets(float dmg) {
        foreach (Hurtbox target in inRangeTargets) {
            damageTarget(target, dmg);
        }
    }


    // Protected method to override that does damage effect to inRangeTarget (to be abstracted)
    protected void damageTarget(Hurtbox tgt, float dmg) {
        tgt.onHurtboxDamaged(dmg);
    }


    // Protected method to apply visual effect to targets (to be abstracted)
    protected void applyVisualEffects() {}

}
