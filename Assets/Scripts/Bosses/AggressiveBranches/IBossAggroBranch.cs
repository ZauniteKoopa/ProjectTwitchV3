using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class IBossAggroBranch : MonoBehaviour
{
    // Variables
    protected NavMeshAgent navMeshAgent;
    protected ITwitchUnitStatus enemyStats;


    // Main variables for most aggroBranches
    private void Awake() {
        // Get reference variables and error check
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyStats = GetComponent<ITwitchUnitStatus>();

        if (navMeshAgent == null){
            Debug.LogError("No nav mesh agent connected to this unit: " + transform, transform);
        }

        if (enemyStats == null){
            Debug.LogError("No ITwitchUnitStatus connected to this unit: " + transform, transform);
        }

        // Do any branch specific initialization
        initialize();
    }


    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected abstract void initialize();


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public abstract IEnumerator execute(Transform tgt, int phaseNumber);


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public abstract void reset();


    // Main event handler for when enemy has been hard reset and you must clean up any side effects
    //  By default, just reset
    public virtual void hardReset() {
        reset();
    }


    // Main function to check for stop conditions when going to position
    //  Pre: none
    //  Post: returns whether a stop condition has been met
    protected virtual bool reachedStopCondition() {
        return true;
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
            navMeshAgent.speed = enemyStats.getMovementSpeed();

            while (!reachedStopCondition() && timer < pathExpiration) {
                yield return waitFrame;

                navMeshAgent.speed = enemyStats.getMovementSpeed();
                currentDistance = Vector3.Distance(dest, transform.position);
                timer += Time.fixedDeltaTime;
            }
        }

        navMeshAgent.isStopped = true;
    }
}
