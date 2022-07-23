using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ElasticDomeShield : IBattleUltimate
{
    // UI
    [SerializeField]
    private ResourceBar shieldDurationDisplay;

    // Ult properties
    private float elasticDomeDuration;
    private float elasticStunDuration;
    private bool setup = false;

    // Helper variables
    private HashSet<ITwitchUnitStatus> stunned = new HashSet<ITwitchUnitStatus>();
    private readonly object stunnedLock = new object();
    private bool domeActive = false;


    // Main abstract function to set ult properties
    //  Pre: ultParameters.length == 2 in the follwing order [dome duration, stun duration]
    //  Post: ultimate properties have been set up
    public override void setUltimateProperties(float[] ultParameters) {
        Debug.Assert(ultParameters.Length == 2 && ultParameters[0] > 0.0f && ultParameters[1] > 0.0f);

        elasticDomeDuration = ultParameters[0];
        elasticStunDuration = ultParameters[1];
        setup = true;
    }


    // Main function to activate sequence
    //  Pre: setup is marked true
    //  Post: activates ultimate sequence
    public override void activateUltimate() {
        Debug.Assert(setup == true);

        StartCoroutine(elasticDomeSequence());
    }


    // Main IE numerator sequence to do ultimate
    private IEnumerator elasticDomeSequence() {
        // Set timer
        float timer = 0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        domeActive = true;

        // Main timer loop
        while (timer < elasticDomeDuration) {
            yield return waitFrame;
            timer += Time.fixedDeltaTime;

            shieldDurationDisplay.setStatus(elasticDomeDuration - Mathf.Min(timer, elasticDomeDuration), elasticDomeDuration);
        }

        // Deactivate collider, mesh renderer and resource bar
        domeActive = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        shieldDurationDisplay.gameObject.SetActive(false);

        // Wait for however long it takes for stun durations to go down
        yield return new WaitForSeconds(elasticStunDuration + 0.15f);
    }


    // Main IEnumerator to handle stun duration
    private IEnumerator stunEnemy(ITwitchUnitStatus tgt) {
        tgt.stun(true);
        yield return new WaitForSeconds(elasticStunDuration);
        tgt.stun(false);

        // Remove tgt from list of stunned people
        lock (stunnedLock) {
            if (tgt != null && stunned.Contains(tgt)) {
                stunned.Remove(tgt);
            }
        }
    }


    // If an enemy enters, stun the enemy
    private void OnTriggerEnter(Collider collider) {
        ITwitchUnitStatus testEnemy = collider.GetComponent<ITwitchUnitStatus>();

        // Check if it's actually an enemy
        if (testEnemy != null) {

            // Check if stun conditions are even true
            lock (stunnedLock) {
                if (domeActive && !stunned.Contains(testEnemy)) {
                    stunned.Add(testEnemy);
                    StartCoroutine(stunEnemy(testEnemy));
                }
            }
        }
    }


    // If an enemy enters, stun the enemy
    private void OnTriggerExit(Collider collider) {
        ITwitchUnitStatus testEnemy = collider.GetComponent<ITwitchUnitStatus>();

        // Check if it's actually an enemy
        if (testEnemy != null) {

            // Check if stun conditions are even true
            lock (stunnedLock) {
                if (domeActive && !stunned.Contains(testEnemy)) {
                    stunned.Add(testEnemy);
                    StartCoroutine(stunEnemy(testEnemy));
                }
            }
        }
    }


    

}
