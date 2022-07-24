using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PulsatingCaskZone : IBattleUltimate
{
    // Ult parameters
    private float caskSlow;
    private float pullDistance;
    private float pullDelay;
    private bool setup = false;

    //Enums to represet the stages of the cask
    private enum PulsatingCaskStage {
        ATTACHING,
        PULLING,
        DEAD
    };

    // Variables to manage the state of pulsating cask
    private PulsatingCaskStage currentStage = PulsatingCaskStage.ATTACHING;
    private HashSet<ITwitchUnitStatus> attached = new HashSet<ITwitchUnitStatus>();
    private const float PULL_TIME = 0.35f;


    // Main abstract function to set ult properties
    //  Pre: ultParameter.Length == 3, [caskSlow, pullDistance, pullDelay]
    //  Post: ultimate properties have been set up
    public override void setUltimateProperties(float[] ultParameters) {
        Debug.Assert(ultParameters.Length == 3);
        Debug.Assert(ultParameters[0] > 0.0f && ultParameters[0] < 1.0f);
        Debug.Assert(ultParameters[1] > 0.0f && ultParameters[2] > 0.0f);

        setup = true;
        caskSlow = ultParameters[0];
        pullDistance = ultParameters[1];
        pullDelay = ultParameters[2];
    }


    // Main function to activate sequence
    //  Pre: setup is true
    //  Post: activates ultimate sequence
    public override void activateUltimate() {
        Debug.Assert(setup == true);
        StartCoroutine(ultimateSequence());
    }


    // Main sequence for handling pulsating cask
    private IEnumerator ultimateSequence() {
        // Attach
        currentStage = PulsatingCaskStage.ATTACHING;
        yield return new WaitForSeconds(pullDelay);

        // Pull: stun all targets first and calculate their pull vectors
        currentStage = PulsatingCaskStage.PULLING;
        Dictionary<ITwitchUnitStatus, Vector3> finalLocations = new Dictionary<ITwitchUnitStatus, Vector3>();
        Dictionary<ITwitchUnitStatus, Vector3> startLocations = new Dictionary<ITwitchUnitStatus, Vector3>();

        foreach (ITwitchUnitStatus tgt in attached) {
            if (tgt != null) {
                tgt.stun(true);
                startLocations.Add(tgt, tgt.transform.position);
                
                Vector3 flatCaskPosition = new Vector3(transform.position.x, tgt.transform.position.y, transform.position.z);
                Vector3 currentFinalLocation = tgt.transform.position + (pullDistance * (flatCaskPosition - tgt.transform.position).normalized);
                finalLocations.Add(tgt, currentFinalLocation);
            }
        }

        // Actual while loop for pulling, disable mesh before pulling
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        float pullTimer = 0f;
        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;

        while (pullTimer < PULL_TIME) {
            yield return waitFrame;
            pullTimer += Time.fixedDeltaTime;

            foreach (ITwitchUnitStatus tgt in attached) {
                if (tgt != null) {
                    tgt.transform.position = Vector3.Lerp(startLocations[tgt], finalLocations[tgt], pullTimer / PULL_TIME);
                }
            }            
        }

        // Unstun and remove slow debuff on any remaining living units
        foreach (ITwitchUnitStatus tgt in attached) {
            if (tgt != null) {
                tgt.stun(false);
                tgt.affectSpeed(1.0f / caskSlow); 
            }
        }

        currentStage = PulsatingCaskStage.DEAD;
        Object.Destroy(gameObject);
    }


    // On trigger enter, get attached if in appropriate stage
    private void OnTriggerEnter(Collider collider) {
        if (currentStage == PulsatingCaskStage.ATTACHING) {
            ITwitchUnitStatus testEnemy = collider.GetComponent<ITwitchUnitStatus>();

            // Check if it's actually an enemy
            if (testEnemy != null && !attached.Contains(testEnemy)) {
                attached.Add(testEnemy);
                testEnemy.affectSpeed(caskSlow);             
            }
        }
    }
}
