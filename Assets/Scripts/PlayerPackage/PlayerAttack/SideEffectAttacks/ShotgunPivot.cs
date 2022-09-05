using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShotgunPivot : MonoBehaviour, ITwitchBasicAttack
{
    [SerializeField]
    private PoisonMeleeAttack poisonBlast;
    private float blastDuration = 0.35f;


    // Main function to connect basic attack damage to this new poison
    //  Pre: newPoison CAN be null or non-null
    //  Post: poison vial is now connected to this attack to calculate damage. Damage calculations depend on the basic attack instance
    public void setVialDamage(IVial newPoison, float damageMultiplier) {
        Debug.Assert(damageMultiplier > 0.0f);
        poisonBlast.setVialDamage(newPoison, damageMultiplier);
    }


    // Main function to fire the basic attack towards a certain direction
    //  Pre: projDir is the direction that the projectile is facing and projSpeed is how fast the projectile goes
    //  Post: sets up basic attack to go a certain direction
    public void setUpMovement(Vector3 projDir, float projSpeed, IAimAssist aimer = null) {
        transform.forward = projDir;
        StartCoroutine(blastSequence());
    }


    // Blast sequence
    private IEnumerator blastSequence() {
        yield return new WaitForSeconds(blastDuration);
        Object.Destroy(gameObject);
    }



    // Main function to get access to the transform of this component
    //  Pre: none
    //  Post: returns the transform associated with this object
    public Transform getTransform() {
        return transform;
    }
}
