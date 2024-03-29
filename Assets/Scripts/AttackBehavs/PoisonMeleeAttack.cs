using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PoisonMeleeAttack : MonoBehaviour
{
    private IVial poison;
    private HashSet<ITwitchUnitStatus> hit = new HashSet<ITwitchUnitStatus>();
    private float meleeAttackDuration = 0.3f;
    private float damageMultiplier = 1.0f;


    // On awake, start melee attack sequence
    private void Awake() {
        StartCoroutine(meleeAttackSequence());
    }

    // Main melee attack sequence
    private IEnumerator meleeAttackSequence() {
        gameObject.SetActive(true);
        yield return new WaitForSeconds(meleeAttackDuration);
        gameObject.SetActive(false);
        hit.Clear();
    }


    // Main function to connect basic attack damage to this new poison
    //  Pre: newPoison CAN be null or non-null
    //  Post: poison vial is now connected to this attack to calculate damage. Damage calculations depend on the basic attack instance
    public void setVialDamage(IVial newPoison, float multiplier) {
        Debug.Assert(multiplier > 0.0f);

        poison = newPoison;
        damageMultiplier = multiplier;
    }


    // On TriggerEnter
    private void OnTriggerEnter(Collider collider) {
        ITwitchUnitStatus colliderHurtbox = collider.GetComponent<ITwitchUnitStatus>();
        IHittable stageElementHurtbox = collider.GetComponent<IHittable>();

        if (colliderHurtbox != null && !hit.Contains(colliderHurtbox)) {
            hit.Add(colliderHurtbox);
            colliderHurtbox.poisonDamage(poison.getBoltDamage(0) * damageMultiplier, poison, 1);
        }

        if (stageElementHurtbox != null) {
            stageElementHurtbox.hit();
        }
    }
}
