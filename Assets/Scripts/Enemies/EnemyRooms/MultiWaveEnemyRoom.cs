using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

// Main helper class to serialize a spawnIn enemy wave
[System.Serializable]
public class EnemyWave {

    // List of enemies
    [SerializeField]
    private ITwitchUnitStatus[] enemies;
    private bool spawned = false;


    // Main function to validate enemy room to see if it meets requirements: enemies.Length > 0 && no enemies in the array or null
    //  Pre: none
    //  Post: returns true if room is considered valid. Returns false if it didn't
    public bool validateRoom() {
        // Check that wave actually has enemies in it
        if (enemies.Length == 0) {
            return false;
        }

        // Check that no enemies inside are null
        foreach (ITwitchUnitStatus enemy in enemies) {
            if (enemy == null) {
                return false;
            }
        }

        return true;
    }


    // Main function to spawn enemy wave if not spawned already
    //  Pre: None
    //  Post: spawns enemies accordingly
    public void spawn() {
        if (!spawned) {
            spawned = true;

            foreach (ITwitchUnitStatus enemy in enemies) {
                enemy.spawnIn();
            }
        }
    }


    // Main function to reset enemy wave 
    //  Pre: none
    //  Post: resets wave as if it never happened
    public void reset() {
        spawned = false;

        foreach (ITwitchUnitStatus enemy in enemies) {
            enemy.despawn();
        }
    }


    // Main function to add death listener
    //  Pre: deathListener != null
    //  Post: deathListener will listen when any enemy has died within this wave
    public void addDeathListener(UnityAction<IUnitStatus> deathListener) {
        Debug.Assert(deathListener != null);

        foreach (ITwitchUnitStatus enemy in enemies) {
            enemy.unitDeathEvent.AddListener(deathListener);
        }
    }


    // Main accessor function to get the number of enemies within this wave
    //  Pre: none
    //  Post: returns a value greater than 0
    public int getNumEnemies() {
        int numEnemies = enemies.Length;

        Debug.Assert(numEnemies > 0);
        return numEnemies;
    }

}




// Main class to handle multiwave enemy rooms
public class MultiWaveEnemyRoom : IEnemyGroup
{
    // Serialized fields
    [SerializeField]
    private EnemyWave[] enemyWaves;
    [SerializeField]
    private IHittable[] hittableStageElements;
    [SerializeField]
    private Camera roomCamera;

    // Unity events
    public UnityEvent allEnemiesDeadEvent;
    public UnityEvent resetEvent;
    public UnityEvent triggerRoomEvent;

    // Enemy management
    private Collider roomTrigger = null;
    private int curWaveNumber = 0;
    private int numEnemiesLeft = 0;
    private readonly object enemyLock = new object();


    // Start is called before the first frame update
    void Start()
    {
        // Set up trigger box
        roomTrigger = GetComponent<Collider>();
        if (roomTrigger == null) {
            Debug.LogError("No trigger box collider found for triggering the room", transform);
        }

        // Deactivate cameras for now since they're mainly there for the designers
        if (roomCamera != null && roomCamera.transform.parent != transform) {
            Debug.LogError("Room camera SHOULD be connect to the enemy room transform as a parent or not exist at all if you don't want camera transitions", transform);
        }

        if (roomCamera != null) {
            roomCamera.gameObject.SetActive(false);
        }

        // Error check the waves themselves and set them up
        if (enemyWaves.Length == 0) {
            Debug.LogError("MULTIWAVE ROOM HAS NO WAVES", transform);
        }

        for(int i = 0; i < enemyWaves.Length; i++) {
            EnemyWave curEnemyWave = enemyWaves[i];
            if (!curEnemyWave.validateRoom()) {
                Debug.LogError("ERROR FOUND IN WAVE " + i + " OF MULTIWAVE ROOM. PLEASE MAKE SURE THE WAVE ISN'T EMPTY OR THAT ALL ENEMIES WITHIN THE WAVE ARE NON-NULL (not none)", transform);
            }

            curEnemyWave.reset();
            curEnemyWave.addDeathListener(onEnemyKilled);
        }

        // set up wave data
        lock (enemyLock) {
            curWaveNumber = 0;
            numEnemiesLeft = enemyWaves[0].getNumEnemies();
        }

        // Trigger reset event so that everything is appropriate by first frame
        resetEvent.Invoke();
    }


    // Main function to reset enemy group
    //  Pre: none, player dies and respawns to check point associated with this group
    //  Post: resets the room as if nothing has really happened
    public override void reset() {
        // go through all enemies and despawn them
        foreach (EnemyWave wave in enemyWaves) {
            wave.reset();
        }

        // Edit wave data
        lock (enemyLock) {
            curWaveNumber = 0;
            numEnemiesLeft = enemyWaves[0].getNumEnemies();
        }

        // Reset stage elements
        foreach(IHittable stageElement in hittableStageElements) {
            stageElement.reset();
        }

        // Reset any doors associated
        resetEvent.Invoke();
        roomTrigger.enabled = true;
    }


    // Event handler function to handle when an enemy belong to this room has been killed
    //  Pre: an enemy belong to this enemy group has been killed, represented by its sent status
    //  Post: keeps track of how many enemies killed. If all enemies killed currently, do something
    public override void onEnemyKilled(IUnitStatus status) {
        // edit wave data on killed
        lock (enemyLock) {
            numEnemiesLeft--;

            // If no more enemies in the wave left, either go to the next wave or finish the enemy room
            if (numEnemiesLeft <= 0) {
                curWaveNumber++;

                // Case where you completed the enemy room
                if (curWaveNumber >= enemyWaves.Length) {
                    allEnemiesDeadEvent.Invoke();
                    PlayerCameraController.reset();

                // Case where you go to the next wave
                } else {
                    numEnemiesLeft = enemyWaves[curWaveNumber].getNumEnemies();
                    enemyWaves[curWaveNumber].spawn();
                }
            }
        }
    }


    // Main trigger for when player enter the room
    private void OnTriggerEnter(Collider collider) {
        ITwitchStatus playerStatus = collider.GetComponent<ITwitchStatus>();

        if (playerStatus != null) {
            roomTrigger.enabled = false;
            triggerRoomEvent.Invoke();

            // Spawn in enemies
            lock (enemyLock) {
                curWaveNumber = 0;
                numEnemiesLeft = enemyWaves[curWaveNumber].getNumEnemies();
                enemyWaves[curWaveNumber].spawn();
            }

            // Move camera
            if (roomCamera != null) {
                PlayerCameraController.moveCamera(transform, roomCamera.transform.localPosition, roomCamera.transform.localRotation);
            }
        }
    }
}
