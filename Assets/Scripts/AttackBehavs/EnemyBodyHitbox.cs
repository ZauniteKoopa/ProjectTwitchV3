using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBodyHitbox : MonoBehaviour
{
    public UnityEvent damageTargetEvent;
    private float bodyDamage;

    // Main OnTriggerStay event: do damage to unit on every frame
    private void OnTriggerStay(Collider collider) {
        // get appropriate IUnitStatus from this collider
        IUnitStatus tgt = collider.GetComponent<IUnitStatus>();

        if (tgt != null && tgt.damage(bodyDamage)) {
            damageTargetEvent.Invoke();
        }
    }


    // Main function to initialize hitbox damage
    public void init(float damage) {
        bodyDamage = damage;
    }
}
