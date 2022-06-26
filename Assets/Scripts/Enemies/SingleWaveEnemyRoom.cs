using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SingleWaveEnemyRoom : IEnemyGroup
{
    // Private instance variable: collection of enemies to keep track of
    [SerializeField]
    private ITwitchUnitStatus[] enemies;
    [SerializeField]
    private IHittable[] hittableStageElements;
    private int numEnemiesLeft;
    public UnityEvent allEnemiesDeadEvent;
    public UnityEvent resetEvent;

    private readonly object enemyLock = new object();

    // Start is called before the first frame update
    void Awake()
    {
        numEnemiesLeft = enemies.Length;

        foreach(ITwitchUnitStatus enemy in enemies) {
            enemy.unitDeathEvent.AddListener(onEnemyKilled);
        }
    }

    // Main function to reset enemy group
    //  Pre: none, player dies and respawns to check point associated with this group
    //  Post: resets the room as if nothing has really happened
    public override void reset() {
        // reset enemies
        numEnemiesLeft = enemies.Length;
        foreach(ITwitchUnitStatus enemy in enemies) {
            enemy.reset();
        }

        // Reset stage elements
        foreach(IHittable stageElement in hittableStageElements) {
            stageElement.reset();
        }

        // Reset any doors associated
        resetEvent.Invoke();
    }


    // Event handler function to handle when an enemy belong to this room has been killed
    //  Pre: an enemy belong to this enemy group has been killed
    //  Post: keeps track of how many enemies killed. If all enemies killed currently, do something
    public override void onEnemyKilled() {

        // Add lock for secure synch
        lock(enemyLock) {
            numEnemiesLeft--;

            if (numEnemiesLeft <= 0) {
                allEnemiesDeadEvent.Invoke();
            }
        }
    }
}
