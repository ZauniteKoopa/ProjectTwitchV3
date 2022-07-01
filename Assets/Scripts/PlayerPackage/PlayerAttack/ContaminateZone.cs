using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContaminateZone : AbstractDamageZone
{
    // Initialize access to the mesh renderer
    private MeshRenderer meshRender;
    [SerializeField]
    private ContaminateVisionTrigger visionTrigger;


    // Initializing variables and event connections
    protected override void initialize() {
        // Set up mesh renderer
        meshRender = GetComponent<MeshRenderer>();
        if (meshRender == null) {
            Debug.LogError("No mesh renderer connected");
        }

        meshRender.enabled = false;

        // Set up visionTrigger
        if (visionTrigger == null) {
            Debug.LogError("No contaminate vision trigger connected");
        }

        visionTrigger.enemyPoisonFoundEvent.AddListener(onEnemyPoisonNearby);
        visionTrigger.enemyPoisonGoneEvent.AddListener(onAllPoisonedEnemiesGone);
    }


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


    // Main event handler for when someone starts getting poisoned nearby
    private void onEnemyPoisonNearby() {
        meshRender.enabled = true;
    }


    // Main event handler for when someone starts getting poisoned nearby
    private void onAllPoisonedEnemiesGone() {
        meshRender.enabled = false;
    }

}
