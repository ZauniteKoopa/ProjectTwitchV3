using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class BossPhaseDelegate : UnityEvent<int> {}

public class BossStatus : TwitchEnemyStatus
{
    // Array to handle the number of phases
    [Header("Boss Information")]
    [SerializeField]
    private float[] phaseHealthPercents;
    private float curPhaseThreshold;
    private float totalHealthPercentPassed;
    private int curPhase;

    // Main flag for phase transitioning
    [SerializeField]
    private float phaseTransitionTime = 1.5f;
    public BossPhaseDelegate transitionPhaseStartEvent;
    public UnityEvent transitionPhaseEndEvent;
    private Coroutine transitionSequence;

    private bool phaseTransitioning;
    private MeshRenderer meshRender;
    private Color originalColor;

    // UI
    [SerializeField]
    private ResourceBar phaseThresholdBar;


    // Main function to handle initialization
    protected override void initialize() {
        // Error check phaseHealthPercents
        if (phaseHealthPercents.Length == 0) {
            Debug.LogError("No phase health percents logged in this boss status. What phases?", transform);
        }

        float totalPercent = 0f;
        foreach (float healthPercent in phaseHealthPercents) {
            totalPercent += healthPercent;
        }

        if (totalPercent < 0.9999 || totalPercent > 1.0001) {
            Debug.LogError("Make sure that all phase health percents for this boss statuses add up to 1.0", transform);
        }

        // Set up boss phase
        curPhase = 0;
        curPhaseThreshold = maxHealth - (maxHealth * phaseHealthPercents[curPhase]);
        totalHealthPercentPassed = phaseHealthPercents[curPhase];
        phaseThresholdBar.setStatus(curPhaseThreshold, maxHealth);

        meshRender = GetComponent<MeshRenderer>();
        originalColor = meshRender.material.color;
    }


    // Main protected function to check execution
    //  Pre: none, poisonLock has been obtained
    //  Post: if you meet infected vial's conditions, kill this unit immediately
    protected override void checkAutoExecution(IVial pv, int pStacks) {
        Debug.Assert(pStacks >= 0 && pStacks <= 6);

        if (pv != null) {
            if (pv.canAutoExecute(true, curHealth / maxHealth, pStacks)) {
                damage(curHealth, true);
            }
        }
    }


    // Main damage function to override
    //  Pre: damage is a number greater than 0
    //  Post: unit gets inflicted with damage and returns if damage was successful
    public override bool damage(float dmg, bool isTrue) {
        bool didDamage = (!phaseTransitioning) ? base.damage(dmg, isTrue) : false;

        // Check if you're transitioning to another phase
        lock (healthLock) {
            if (isAlive() && curHealth <= curPhaseThreshold) {
                curPhase++;
                totalHealthPercentPassed += phaseHealthPercents[curPhase];
                curPhaseThreshold = maxHealth - (maxHealth * totalHealthPercentPassed);
                phaseThresholdBar.setStatus(curPhaseThreshold, maxHealth);

                transitionSequence = StartCoroutine(transitionPhaseSequence());
            }
        }

        return didDamage;
    }


    // Phase transitioning sequence
    //  Pre: boss has moved on to the next phase
    //  Post: Boss becomes invulnerable to all damage and enemy behavior stops temporarily
    private IEnumerator transitionPhaseSequence() {
        transitionPhaseStartEvent.Invoke(curPhase);
        phaseTransitioning = true;
        meshRender.material.color = Color.blue;
        dropLoot();

        yield return new WaitForSeconds(phaseTransitionTime);

        transitionSequence = null;
        meshRender.material.color = originalColor;
        phaseTransitioning = false;
        transitionPhaseEndEvent.Invoke();
    }


    // Main function to check if the unit is transitioning to a new phase
    //  Pre: none
    public bool isPhaseTransitioning() {
        return phaseTransitioning;
    }


    // Main function to reset unit, especially when player dies
    //  Pre: none
    //  Post: If enemy, reset to passive state, not sensing any enemies
    //        If player, reset all cooldowns to 0 and lose collectibles upon death
    public override void reset() {
        base.reset();

        // Stop transition sequence
        if (transitionSequence != null) {
            StopCoroutine(transitionSequence);
            transitionSequence = null;
        }

        phaseTransitioning = false;

        // Change phase variables
        curPhase = 0;
        totalHealthPercentPassed = phaseHealthPercents[curPhase];
        curPhaseThreshold = maxHealth - (maxHealth * totalHealthPercentPassed);
        phaseThresholdBar.setStatus(curPhaseThreshold, maxHealth);

    }
}
