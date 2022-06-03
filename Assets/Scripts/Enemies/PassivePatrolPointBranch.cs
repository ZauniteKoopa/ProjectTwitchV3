using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PassivePatrolPointBranch : IEnemyPassiveBranch
{
    private int patrolPointIndex = 0;
    private List<Vector3> patrolPointLocations;
    private NavMeshAgent navMeshAgent;
    private bool wasReset = true;
    [SerializeField]
    private Transform[] patrolPoints;
    [SerializeField]
    private float nearDistance = 0.25f;
    [SerializeField]
    private float stopDuration = 1.0f;

    [SerializeField]
    private Transform staticUI = null;
    private Vector3 uiForward;


    // On awake, set patrolPointLocations immediately and get NavMeshAgent
    private void Awake() {
        // Error check
        if (patrolPoints.Length <= 0) {
            Debug.LogError("No patrol points connected to patrol point branch");
        }

        // Initialize variables
        navMeshAgent = GetComponent<NavMeshAgent>();
        patrolPointLocations = new List<Vector3>();
        float yPos = transform.position.y;

        if (staticUI != null) {
            uiForward = staticUI.forward;
        }

        // record each patrol point location
        foreach (Transform patrolPoint in patrolPoints) {
            patrolPointLocations.Add(new Vector3(patrolPoint.position.x, yPos, patrolPoint.position.z));
            patrolPoint.gameObject.SetActive(false);
        }
    }


    // Main function to run the branch
    public override IEnumerator execute() {

        // If this is the first time running branch or branch was reset, find nearest patrol point to start
        if (wasReset) {
            wasReset = false;
            patrolPointIndex = getNearestPatrolPoint();
        } else {
            patrolPointIndex = 0;
        }

        navMeshAgent.isStopped = false;

        // Go through the entire path in chronological order
        while (patrolPointIndex < patrolPointLocations.Count) {
            // Set destination
            Vector3 destPos = patrolPointLocations[patrolPointIndex];
            yield return StartCoroutine(goToPosition(destPos));

            // Wait for stop duration
            yield return new WaitForSeconds(stopDuration);

            // Increment patrol point index and start over
            patrolPointIndex++;
        }
    }


    // Main function to get unit to go to a specific location
    //  Pre: dest is the position on the nav mesh that the unit is trying to go to
    //  Post: unit will move to the position gradually, getting out of sequence once reached position
    protected IEnumerator goToPosition(Vector3 dest) {
        bool pathFound = navMeshAgent.SetDestination(dest);

        // If path found, go to path
        if (pathFound) {
            // Variable to represent waiting a frame and set starting location
            WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

            // Wait for path to finish calculating
            while (navMeshAgent.pathPending) {
                yield return waitFrame;
            }

            // Wait for unit to approach destination
            float currentDistance = Vector3.Distance(dest, transform.position);
            while (currentDistance > nearDistance) {
                yield return waitFrame;

                if (staticUI != null) {
                    staticUI.forward = uiForward;
                }

                currentDistance = Vector3.Distance(dest, transform.position);
            }
        }
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        wasReset = true;
        navMeshAgent.isStopped = true;
    }


    // Main helper function to get the index of the nearest patrol point location
    private int getNearestPatrolPoint() {
        float minDistance = Vector3.Distance(transform.position, patrolPointLocations[0]);
        int closestIndex = 0;

        for (int i = 1; i < patrolPointLocations.Count; i++) {
            float curDistance = Vector3.Distance(transform.position, patrolPointLocations[i]);

            if (minDistance > curDistance) {
                minDistance = curDistance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }
}
