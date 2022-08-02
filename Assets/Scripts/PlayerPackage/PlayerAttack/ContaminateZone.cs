using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContaminateZone : AbstractDamageZone
{
    // Initialize access to the mesh renderer
    private MeshRenderer meshRender;
    [SerializeField]
    private ContaminateVisionTrigger visionTrigger;
    [SerializeField]
    private IFixedEffect visualEffectPrefab;
    [SerializeField]
    private float vfxDuration = 0.15f;
    private bool poisonedEnemiesNearby = false;
    private bool contaminateReady = true;


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
        StartCoroutine(damageSequence(tgt));
    }


    // Main damage coroutine
    private IEnumerator damageSequence(ITwitchUnitStatus tgt) {
        yield return new WaitForSeconds(vfxDuration);
        tgt.contaminate();
        checkDeath(tgt);
    }


    // Protected method to apply visual effect to targets
    protected override void applyVisualEffects(ITwitchUnitStatus tgt) {
        // Very duct tape solution because abstract enemy field doesn't remove on death
        if (tgt.isPoisoned() && tgt.gameObject.activeInHierarchy) {
            IFixedEffect currentLob = Object.Instantiate(visualEffectPrefab, transform.position, Quaternion.identity);
            currentLob.activateEffect(transform.position, tgt.transform.position, vfxDuration);
        }
    }


    // Protected method to check if you can use damage zone
    //  Returns true IFF at least 1 POISONED enemy in range
    public override bool canUseAbility() {
        bool canUse = false;

        lock(targetsLock) {
            foreach(ITwitchUnitStatus tgt in inRangeTargets) {
                canUse = tgt.isPoisoned() && tgt.gameObject.activeInHierarchy;

                if (canUse) {
                    break;
                }
            }
        }

        return canUse;
    }


    // Main event handler for when someone starts getting poisoned nearby
    private void onEnemyPoisonNearby() {
        poisonedEnemiesNearby = true;
        updateVisibility();
    }


    // Main event handler for when someone starts getting poisoned nearby
    private void onAllPoisonedEnemiesGone() {
        poisonedEnemiesNearby = false;
        updateVisibility();
    }

    // Main event handler for when contaminate ability is ready
    public void onContaminateReady() {
        contaminateReady = true;
        updateVisibility();
    }


    // Main event handler for when contamin ability is used
    public void onContaminateUsed() {
        contaminateReady = false;
        updateVisibility();
    }


    // Private helper function to update visibility
    private void updateVisibility() {
        meshRender.enabled = contaminateReady && poisonedEnemiesNearby;
    }

}
