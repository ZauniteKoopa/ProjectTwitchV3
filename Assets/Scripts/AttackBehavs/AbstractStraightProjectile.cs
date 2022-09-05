using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractStraightProjectile : MonoBehaviour
{
    // Private helper function to do movement
    private Vector3 projectileDir;
    private float projectileSpeed = 0.0f;
    private float timeoutDistance = 30f;

    // Movement tracking
    private bool moving = false;
    private float curDistanceTraveled = 0.0f;

    // Aim assister if one needs to adjust the aim
    private IAimAssist aimAssist = null;
    private const float ANGLE_CHANGE_THRESHOLD = 30f;


    // Main method to move the projectile after every frame
    private void FixedUpdate() {
        if (moving) {
            // Calculate movement values
            Vector3 moveVector = projectileDir * projectileSpeed * Time.fixedDeltaTime;
            float moveDistance = moveVector.magnitude;

            // Apply movement values
            transform.Translate(moveVector, Space.World);
            curDistanceTraveled += moveDistance;

            // See if projectile timed out
            if (curDistanceTraveled > timeoutDistance) {
                Object.Destroy(gameObject);
            }
        }
    }


    // Public method to set up the projectile movement properties and start movement
    public void setUpMovement(Vector3 projDir, float projSpeed, IAimAssist aimer = null) {
        // Set projectile movement variables
        projectileDir = projDir.normalized;
        projectileSpeed = projSpeed;
        transform.forward = projDir;

        // Set aimAssist if it exist
        aimAssist = aimer;

        // set moving flag to true
        moving = true;
    }


    // Protected function for children to adjust aim accordingly, is mostly use for polish reasons (Piercing Poison Vial Bolt, possibly reflecting projectiles)
    //  Pre: aimAssit != null, if it is return nothing. excludedEnemy can be null or non-null
    //  Post: adjust aim so that it can hit an enemy at that direction
    protected void adjustAimDirection(ITwitchUnitStatus excludedEnemy) {
        if (aimAssist != null) {
            Vector3 protoAim = aimAssist.adjustAim(projectileDir, transform.position, excludedEnemy);
            if (Vector3.Angle(protoAim, projectileDir) <= ANGLE_CHANGE_THRESHOLD) {
                projectileDir = protoAim;
            }
        }
    }



    // onTriggerEnter function for projectile
    //  MUST only consider solid enviornment (walls) or hurtboxes
    private void OnTriggerEnter(Collider collider) {
        ITwitchUnitStatus colliderHurtbox = collider.GetComponent<ITwitchUnitStatus>();
        IHittable stageElementHurtbox = collider.GetComponent<IHittable>();

        // If hit a valid hurtbox, do damage to enemy. Else, destroy projectile
        if (colliderHurtbox != null) {
            damageTarget(colliderHurtbox);
            onTargetHit();
        } else {
            // If hit something that's hittable, hit it
            if (stageElementHurtbox != null) {
                stageElementHurtbox.hit();
            }

            // Destroy gameobject afterwards
            Object.Destroy(gameObject);
        }
    }

    
    // Main method to do damage to this target
    protected abstract void damageTarget(ITwitchUnitStatus target);

    
    // Main function to handle what happens to projectile body when hitting an enemy
    protected abstract void onTargetHit();
}
