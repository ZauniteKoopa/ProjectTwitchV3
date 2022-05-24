using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbstractStraightProjectile : MonoBehaviour
{
    // Private helper function to do movement
    private Vector3 projectileDir;
    private float projectileSpeed = 0.0f;
    private float timeoutDistance = 30f;

    // Movement tracking
    private bool moving = false;
    private float curDistanceTraveled = 0.0f;


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


    // Public method to set up the projectile movement properties
    public void setUpMovement(Vector3 projDir, float projSpeed) {
        projectileDir = projDir.normalized;
        projectileSpeed = projSpeed;
        moving = true;
    }


    // onTriggerEnter function for projectile
    //  MUST only consider solid enviornment (walls) or hurtboxes
    private void OnTriggerEnter(Collider collider) {
        Object.Destroy(gameObject);
    }

}
