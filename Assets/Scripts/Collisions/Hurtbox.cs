using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Damage event for when this hurtbox senses something
[System.Serializable]
public class HurtboxDamageDelegate : UnityEvent<float> {}

// General hurtbox that's used to do damage to the assigned unit
//  PRE" make sure that collision layers are set to the appropriate layer
public class Hurtbox : MonoBehaviour
{
    public HurtboxDamageDelegate hurtboxDamageEvent;
    private bool activated = true;


    // Main function to activate hurtbox, allowing it to receive damage
    public void activate() {
        activated = true;
    }


    // Main function to deactivate hurtbox, disabling damage and making this hurtbox component invincible
    public void deactivate() {
        activated = false;
    }


    // Main function to deal damage to enemy affected by unit
    public void onHurtboxDamaged(float dmg) {
        if (activated) {
            hurtboxDamageEvent.Invoke(dmg);
        }
    }

}
