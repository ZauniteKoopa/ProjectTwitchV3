using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class CrushbotAggroBranch : IEnemyAggroBranch
{
    private ITwitchUnitStatus enemyStats;

    [Header("Hitbox Variables")]
    [SerializeField]
    private EnemyBodyHitbox bodyHitbox;
    private bool hitPlayer = false;

    [Header("Navigation Sequence Variables")]
    [SerializeField]
    private float surprisedDuration = 0.5f;
    [SerializeField]
    private float pathExpirationInterval = 1.5f;
    private NavMeshAgent navMeshAgent;

    [Header("Recoil Variables")]
    [SerializeField]
    private float recoilDistance = 1f;
    [SerializeField]
    private float recoilStunDuration = 2.5f;
    [SerializeField]
    private float recoilKnockbackDuration = 0.5f;


    // On awake, initialize
    private void Awake() {
        // Error check
        if (bodyHitbox == null) {
            Debug.LogError("No body hitbox connected to crushbot!: " + transform, transform);
        }

        // Get reference variables and error check
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyStats = GetComponent<ITwitchUnitStatus>();

        if (navMeshAgent == null){
            Debug.LogError("No nav mesh agent connected to this unit: " + transform, transform);
        }

        if (enemyStats == null){
            Debug.LogError("No ITwitchUnitStatus connected to this unit: " + transform, transform);
        }

        // Hitbox
        bodyHitbox.init(enemyStats.getBaseAttack());
        bodyHitbox.damageTargetEvent.AddListener(onPlayerHitEnemy);
    }


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt) {
        // If this is the first time running this branch after override, look at enemy
        transform.forward = (tgt.position - transform.position).normalized;
        yield return new WaitForSeconds(surprisedDuration);

        // Go into chasing sequence that will last forever
        while (true) {
            hitPlayer = false;

            // While player not hit, keep chasing player
            while (!hitPlayer) {
                Vector3 currentDestination = tgt.position;
                yield return StartCoroutine(goToPosition(currentDestination, pathExpirationInterval));
            }

            // Once player has been hit, recoil
            Vector3 flattenPos = new Vector3(transform.position.x, tgt.position.y, transform.position.z);
            Vector3 recoilDir = (flattenPos - tgt.position).normalized;
            Vector3 recoilStart = transform.position;
            Vector3 recoilDest = transform.position + (recoilDistance * recoilDir);

            float recoilTimer = 0f;
            WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

            while (recoilTimer <= recoilKnockbackDuration) {
                yield return waitFrame;

                recoilTimer += Time.fixedDeltaTime;
                transform.position = Vector3.Lerp(recoilStart, recoilDest, recoilTimer / recoilKnockbackDuration);
            }

            // Enemy is stunned for fixed duration before moving to enemy again
            yield return new WaitForSeconds(recoilStunDuration);
        }
    }

    // Main function to get unit to go to a specific location
    //  Pre: dest is the position on the nav mesh that the unit is trying to go to, pathExpiration is the time it takes for path to be stale (> 0f)
    //  Post: unit will move to the position gradually, getting out of sequence once reached position. Ends when path expires or hit player
    protected IEnumerator goToPosition(Vector3 dest, float pathExpiration) {
        Debug.Assert(pathExpiration > 0f);

        bool pathFound = navMeshAgent.SetDestination(dest);
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = enemyStats.getMovementSpeed();

        // If path found, go to path
        if (pathFound) {
            // Variable to represent waiting a frame and set starting location
            WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

            // Wait for path to finish calculating
            while (navMeshAgent.pathPending) {
                yield return waitFrame;
            }

            // Wait for unit to either hit the player or path expiration has hit
            float currentDistance = Vector3.Distance(dest, transform.position);
            float timer = 0f;
            while (!hitPlayer && timer < pathExpiration) {
                yield return waitFrame;

                currentDistance = Vector3.Distance(dest, transform.position);
                timer += Time.fixedDeltaTime;
            }
        }

        navMeshAgent.isStopped = true;
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        StopAllCoroutines();
    }


    // Main event handler function for when this enemy hits the player
    public void onPlayerHitEnemy() {
        hitPlayer = true;
    }
}
