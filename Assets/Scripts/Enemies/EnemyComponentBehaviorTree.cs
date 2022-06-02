using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class EnemyComponentBehaviorTree : IEnemyBehavior
{
    // Variables to control the tree
    private readonly object treeLock = new object();
    private Transform playerTgt = null;

    [Header("Enemy Branch Components")]
    [SerializeField]
    private IEnemyAggroBranch aggressiveBranch;
    [SerializeField]
    private IEnemyPassiveBranch passiveBranch;

    
    // On awake, start the behavior tree sequence
    private void Awake() {
        // Error check if branches are connected
        if (passiveBranch == null) {
            Debug.LogError("ERROR, passive branch not connected to this behavior tree: " + transform, transform);
        }

        if (aggressiveBranch == null) {
            Debug.LogError("ERROR, aggressive branch not connected to this behavior tree: " + transform, transform);
        }

        // Execute behav tree for the first time
        StartCoroutine(behaviorTreeSequence());
    }

    
    // The main behavior tree sequence
    private IEnumerator behaviorTreeSequence() {
        while (true) {

            // Test to see if unit is aggressive (they are aggressive IFF a playerTgt is found)
            if (playerTgt == null) {
                yield return StartCoroutine(passiveBranch.execute());
            } else {
                yield return StartCoroutine(aggressiveBranch.execute(playerTgt));
            }
        }
    }


    // Main event handler function for when an enemy sensed a player
    //  Pre: player != null, enemy saw player
    public override void onSensedPlayer(Transform player) {
        lock (treeLock) {
            playerTgt = player;
            passiveBranch.reset();

            StopAllCoroutines();
            StartCoroutine(behaviorTreeSequence());
        }
    }


    // Main event handler function for when an enemy lost sight of a player
    //  Pre: enemy lost sight of player and gave up chasing
    public override void onLostPlayer() {
        lock (treeLock) {
            playerTgt = null;
            aggressiveBranch.reset();

            StopAllCoroutines();
            StartCoroutine(behaviorTreeSequence());
        }
    }
}
