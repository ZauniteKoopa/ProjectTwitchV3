using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DualActivatableSwitch : IHittable
{
    // Private instance variables to keep track of whether this is activated or not
    public UnityEvent activateEvent;
    public UnityEvent deactivateEvent;
    private bool activated = false;

    // Colors to set up
    [SerializeField]
    private Color notActivatedColor = Color.yellow;
    [SerializeField]
    private Color activatedColor = Color.green;
    [SerializeField]
    private float switchCooldown = 0.0f;

    private bool canHit = true;
    private Coroutine cooldownCoroutine = null;
    private MeshRenderer meshRender;


    // On awake, set stuff up
    private void Awake() {
        meshRender = GetComponent<MeshRenderer>();
        meshRender.material.color = notActivatedColor;
    }


    // Main function to hit the object with an attack
    //  Pre: none
    //  Post: switch on if it was initially off or off if it was initially on
    public override void hit() {
        if (canHit) {
            // Trigger event
            activated = !activated;
            UnityEvent triggeredEvent = (activated) ? activateEvent : deactivateEvent;
            triggeredEvent.Invoke();

            if (switchCooldown > 0.001f) {
                cooldownCoroutine = StartCoroutine(hitCooldownSequence());
            } else {
                meshRender.material.color = (activated) ? activatedColor : notActivatedColor;
            }
        }
    }


    // Main cooldown coroutine
    private IEnumerator hitCooldownSequence() {
        meshRender.material.color = Color.red;
        canHit = false;

        yield return new WaitForSeconds(switchCooldown);

        canHit = true;
        meshRender.material.color = (activated) ? activatedColor : notActivatedColor;
        cooldownCoroutine = null;
    }


    // Main function to reset IHittable
    //  Pre: none
    //  Post: sets activated back to false and invoke reset event
    public override void reset() {
        if (activated) {
            activated = false;
            canHit = true;
            meshRender.material.color = notActivatedColor;
            deactivateEvent.Invoke();

            // Stop cooldown sequence if there is any
            if (cooldownCoroutine != null) {
                StopCoroutine(cooldownCoroutine);
            }
        }
    }
}
