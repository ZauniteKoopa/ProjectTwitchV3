using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManicGasZone : IBattleUltimate
{
    // Manic intensity
    private bool setup = false;
    private float manicIntensity = 0.5f;
    private float manicGasDuration = 9f;


   // Main abstract function to set ult properties
    //  Pre: ultParameter.Length == 2, [manicIntensity, manicDuration]
    //  Post: ultimate properties have been set up
    public override void setUltimateProperties(float[] ultParameters) {
        Debug.Assert(ultParameters.Length == 2);
        Debug.Assert(ultParameters[0] > 0.0f && ultParameters[0] < 1.0f);
        Debug.Assert(ultParameters[1] > 0.0f);

        setup = true;
        manicIntensity = ultParameters[0];
        manicGasDuration = ultParameters[1];
    }


    // Main function to activate sequence
    //  Pre: setup is true
    //  Post: activates ultimate sequence
    public override void activateUltimate() {
        Debug.Assert(setup == true);
        StartCoroutine(ultimateSequence());
    }

    // Main function to completely reset
    //  Pre: none
    //  Post: reset all effects so that everythig is back to normal
    public override void reset() {
        StopAllCoroutines();
        StartCoroutine(resetSequence());
    }


    // Main sequence for handling pulsating cask
    private IEnumerator ultimateSequence() {
        // Wait for manic duration seconds
        yield return new WaitForSeconds(manicGasDuration);

        // Banish to the shadow world to remove trigger side effects and then destroy object
        transform.position = new Vector3(0f, 10000000000f, 0f);
        yield return new WaitForSeconds(0.15f);
        Object.Destroy(gameObject);
    }


    // IEnumerator to reset
    private IEnumerator resetSequence() {
        transform.position = Vector3.up * 100000000000f;
        yield return new WaitForSeconds(0.1f);
        Object.Destroy(gameObject);
    }


    // On trigger enter
    private void OnTriggerEnter(Collider collider) {
        ITwitchUnitStatus tgt = collider.GetComponent<ITwitchUnitStatus>();

        if (tgt != null) {
            tgt.makeManic(true, manicIntensity);
        }
    }


    // On trigger exit
    private void OnTriggerExit(Collider collider) {
        ITwitchUnitStatus tgt = collider.GetComponent<ITwitchUnitStatus>();

        if (tgt != null) {
            tgt.makeManic(false, manicIntensity);
        }
    }
}
