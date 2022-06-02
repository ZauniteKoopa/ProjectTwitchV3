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

        // record each patrol point location
        foreach (Transform patrolPoint in patrolPoints) {
            patrolPointLocations.Add(new Vector3(patrolPoint.position.x, yPos, patrolPoint.position.z));
            patrolPoint.gameObject.SetActive(false);
        }
    }


    // Main function to run the branch
    public override IEnumerator execute() {
        // Variable to represent waiting a frame and set starting location
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        // If this is the first time running branch or branch was reset, find nearest patrol point to start
        if (wasReset) {
            patrolPointIndex = getNearestPatrolPoint();
        }

        // Go through the entire path in chronological order
        while (patrolPointIndex < patrolPointLocations.Count) {
            // Set destination
            Vector3 destPos = patrolPointLocations[patrolPointIndex];
            bool pathFound = navMeshAgent.SetDestination(destPos);

            // If path found, go to path
            if (pathFound) {

                // Wait for path to finish calculating
                while (navMeshAgent.pathPending) {
                    yield return waitFrame;
                }

                // Wait for unit to approach destination
                float currentDistance = Vector3.Distance(destPos, transform.position);
                while (currentDistance > nearDistance) {
                    yield return waitFrame;
                }
            }

            // Wait for stop duration
            yield return new WaitForSeconds(stopDuration);

            // Increment patrol point index and start over
            patrolPointIndex++;
        }
    }

    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        wasReset = true;
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
