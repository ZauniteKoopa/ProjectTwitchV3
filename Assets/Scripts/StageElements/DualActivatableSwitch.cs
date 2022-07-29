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
        activated = !activated;
        meshRender.material.color = (activated) ? activatedColor : notActivatedColor;
        UnityEvent triggeredEvent = (activated) ? activateEvent : deactivateEvent;
        triggeredEvent.Invoke();
    }


    // Main function to reset IHittable
    //  Pre: none
    //  Post: sets activated back to false and invoke reset event
    public override void reset() {
        if (activated) {
            activated = false;
            meshRender.material.color = notActivatedColor;
            deactivateEvent.Invoke();
        }
    }
}
