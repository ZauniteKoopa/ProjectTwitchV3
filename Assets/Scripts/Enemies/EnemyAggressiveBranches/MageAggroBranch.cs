using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageAggroBranch : IEnemyAggroBranch
{

    [Header("Navigation Variables")]
    [SerializeField]
    private float surprisedDuration = 0.5f;
    [SerializeField]
    private float pathExpirationInterval = 0.5f;
    [SerializeField]
    private float maxRangedDistance = 6f;
    [SerializeField]
    private float minRangedDistance = 4f;
    [SerializeField]
    private float sideStepRadius = 1.25f;
    private float sideStepNearDistance = 0.1f;

    private bool chasing;
    private float curNavRangedDistance = 0f;
    private Transform target;

    [Header("Attacks")]
    [SerializeField]
    private BasicEnemyProjectile projectile;
    [SerializeField]
    private float projectileAttackAnticipation = 0.25f;
    [SerializeField]
    private float projectileAttackStop = 0.4f;
    [SerializeField]
    private LayerMask eyeSightMask;
    [SerializeField]
    private float projectileSpeed = 14f;


    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize() {
        if (maxRangedDistance < minRangedDistance || minRangedDistance < 0.0f) {
            Debug.LogError("Bad configuration for minRangedDistance and maxRangedDistance for MageAggroBranch", transform);
        }
    }


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt) {
        // If this is the first time running this branch after override, look at enemy
        target = tgt;
        transform.forward = (tgt.position - transform.position).normalized;
        yield return new WaitForSeconds(surprisedDuration);

        // Go into the meat of the branch
        while (true) {
            // Get randomized distance and navigate
            curNavRangedDistance = Random.Range(minRangedDistance, maxRangedDistance);
            chasing = !isInRange();
            Vector3 sidestepDest = transform.position + sideStepRadius * (new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f))).normalized;

            while (!reachedStopCondition()) {
                Vector3 currentDestination = (chasing) ? tgt.position : sidestepDest;
                yield return StartCoroutine(goToPosition(currentDestination, pathExpirationInterval));
            }

            // Face player to attack
            transform.forward = (tgt.position - transform.position).normalized;
            yield return new WaitForSeconds(projectileAttackAnticipation);
            fireProjectile();
            yield return new WaitForSeconds(projectileAttackStop);

        }
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {}


    // Main function to check for stop conditions when going to position
    //  Pre: none
    //  Post: returns whether a stop condition has been met
    protected override bool reachedStopCondition() {
        // If enemy is chasing, just see if you're in range. If in sidestep mode, just see if they reached the side step position
        if (chasing) {
            return isInRange() && navMeshAgent.remainingDistance <= curNavRangedDistance;
        } else {
            return navMeshAgent.remainingDistance <= sideStepNearDistance;
        }
    }


    // Main private helper function to check if you're already in range given by max and min ranged distance
    //  Pre: none
    //  Post: checks if player is already in range
    private bool isInRange() {

        // Get distance
        Vector3 flattenTgt = new Vector3(target.position.x, transform.position.y, target.position.z);
        float distToPlayer = Vector3.Distance(transform.position, flattenTgt);
        bool inProximity = distToPlayer <= maxRangedDistance;

        // Get raycast
        Vector3 rayDir = (flattenTgt - transform.position).normalized;
        bool seeTarget = !Physics.Raycast(transform.position, rayDir, distToPlayer, eyeSightMask);


        return inProximity && seeTarget;
    }


    // Main function to fire the projectile
    //  Pre: none
    //  Post: fires a projectile at the direction of the target
    private void fireProjectile() {
        BasicEnemyProjectile currentProjectile = Object.Instantiate(projectile, transform.position, Quaternion.identity);
        currentProjectile.setDamage(enemyStats.getBaseAttack());
        currentProjectile.setUpMovement(target.position - transform.position, projectileSpeed);
    }
}
