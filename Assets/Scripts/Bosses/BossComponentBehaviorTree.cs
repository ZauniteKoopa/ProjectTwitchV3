using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossComponentBehaviorTree : IEnemyBehavior
{
    // Variables to control the tree
    private readonly object treeLock = new object();
    private Transform playerTgt = null;
    private Vector3 lastSuspectedLocation;

    [Header("Enemy Branch Components")]
    [SerializeField]
    private IBossAggroBranch aggressiveBranch;
    [SerializeField]
    private IBossScoutingBranch scoutingBranch;
    [SerializeField]
    private float branchTransitionTime = 0.5f;
    private NavMeshAgent navMeshAgent;
    private int curPhase = 0;

    private bool transitioning;
    private bool switchedBranches = false;
    private BossStatus unitStatus;


    // On start, start the behavior tree sequence
    private void Start() {
        // Error check if branches are connected
        if (scoutingBranch == null) {
            Debug.LogError("ERROR, scouting branch not connected to this behavior tree: " + transform, transform);
        }

        if (aggressiveBranch == null) {
            Debug.LogError("ERROR, aggressive branch not connected to this behavior tree: " + transform, transform);
        }

        // Execute behav tree for the first time
        navMeshAgent = GetComponent<NavMeshAgent>();
        unitStatus = GetComponent<BossStatus>();

        if (unitStatus == null || navMeshAgent == null) {
            Debug.LogError("No unit status and nav mesh agent found for this enemy", transform);
        }

        unitStatus.enemyResetEvent.AddListener(reset);
        unitStatus.unitDespawnEvent.AddListener(onDespawn);
        unitStatus.unitDeathEvent.AddListener(onDeath);
        unitStatus.transitionPhaseStartEvent.AddListener(onPhaseTransitionStart);
        unitStatus.transitionPhaseEndEvent.AddListener(onPhaseTransitionEnd);
        unitStatus.stunnedStartEvent.AddListener(onStunStart);
        unitStatus.stunnedEndEvent.AddListener(onStunEnd);

        StartCoroutine(behaviorTreeSequence());
    }

    
    // The main behavior tree sequence
    private IEnumerator behaviorTreeSequence() {
        // if you swapped branches naturally, wait for 1.5 seconds to indicate branch trnasition
        if (switchedBranches) {
            switchedBranches = false;
            yield return new WaitForSeconds(branchTransitionTime);
        }

        while (true) {
            // Test to see if unit is aggressive (they are aggressive IFF a playerTgt is found)
            if (playerTgt == null) {
                yield return StartCoroutine(scoutingBranch.execute(lastSuspectedLocation, curPhase));
            } else {
                yield return StartCoroutine(aggressiveBranch.execute(playerTgt, curPhase));
            }
        }
    }


    // Main event handler function for when an enemy sensed a player
    //  Pre: player != null, enemy saw player
    public override void onSensedPlayer(Transform player) {
        lock (treeLock) {
            playerTgt = player;
            scoutingBranch.reset();
            switchedBranches = true;

            if (!transitioning && unitStatus.canMove()) {
                StopAllCoroutines();
                StartCoroutine(behaviorTreeSequence());
            }
        }
    }


    // Main event handler function for when an enemy lost sight of a player
    //  Pre: enemy lost sight of player and gave up chasing
    public override void onLostPlayer() {
        lock (treeLock) {
            lastSuspectedLocation = playerTgt.transform.position;
            playerTgt = null;
            aggressiveBranch.reset();
            switchedBranches = true;

            if (!transitioning && unitStatus.canMove()) {
                StopAllCoroutines();
                StartCoroutine(behaviorTreeSequence());
            }
        }
    }


    // Main function to handle reset
    public override void reset() {
        curPhase = 0;

        lock (treeLock) {
            aggressiveBranch.reset();
            scoutingBranch.reset();

            StopAllCoroutines();
            StartCoroutine(behaviorTreeSequence());
        }
    }


    // Main function to handle when enemy unit despawns
    public void onDespawn() {
        aggressiveBranch.hardReset();
        scoutingBranch.hardReset();
    }


    // Main function for when unit just got stunned
    //  JUst reset both branches
    public void onStunStart() {
        lock (treeLock) {
            StopAllCoroutines();
            aggressiveBranch.reset();
            scoutingBranch.reset();
        }
    }


    // Main function for when unit stun has ended
    //  Just start the coroutine once again
    public void onStunEnd() {
        lock (treeLock) {
            StartCoroutine(behaviorTreeSequence());
        }
    }


    // Main function to handle death
    public override void onDeath(IUnitStatus status) {
        aggressiveBranch.reset();
        scoutingBranch.reset();
        StopAllCoroutines();
    }


    // Main function to handle event for which the enemy is transitioning phases
    public void onPhaseTransitionStart(int newPhase) {
        // Set meta variables
        curPhase = newPhase;
        transitioning = true;

        // Stop behavior
        aggressiveBranch.reset();
        scoutingBranch.reset();
        StopAllCoroutines();
    }


    // Main function to handle event for which boss is exiting transitioning phases
    public void onPhaseTransitionEnd() {
        // Set meta variables
        transitioning = false;

        // Execute tree
        StartCoroutine(behaviorTreeSequence());
    }
}
