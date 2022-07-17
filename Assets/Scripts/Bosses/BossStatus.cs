using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    public UnityEvent transitionPhaseStartEvent;
    public UnityEvent transitionPhaseEndEvent;
    private Coroutine transitionSequence;

    private bool phaseTransitioning;
    private MeshRenderer meshRender;
    private Color originalColor;


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

        meshRender = GetComponent<MeshRenderer>();
        originalColor = meshRender.material.color;
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

                transitionSequence = StartCoroutine(transitionPhaseSequence());
            }
        }

        return didDamage;
    }


    // Phase transitioning sequence
    //  Pre: boss has moved on to the next phase
    //  Post: Boss becomes invulnerable to all damage and enemy behavior stops temporarily
    private IEnumerator transitionPhaseSequence() {
        transitionPhaseStartEvent.Invoke();
        phaseTransitioning = true;
        meshRender.material.color = Color.blue;
        dropLoot();

        yield return new WaitForSeconds(phaseTransitionTime);

        transitionSequence = null;
        meshRender.material.color = originalColor;
        phaseTransitioning = false;
        transitionPhaseEndEvent.Invoke();
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

    }
}
