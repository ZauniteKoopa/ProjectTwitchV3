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
    [SerializeField]
    private bool spawnIn = false;
    [SerializeField]
    private Camera roomCamera;

    private int numEnemiesLeft;
    private Collider roomTrigger = null;
    public UnityEvent allEnemiesDeadEvent;
    public UnityEvent resetEvent;
    public UnityEvent triggerRoomEvent;

    private readonly object enemyLock = new object();

    // Start is called before the first frame update
    void Start()
    {
        // Set up enemies. If this room is set to spawnIn, despawn enemies. They will only spawn in when player enters the room
        numEnemiesLeft = enemies.Length;
        foreach(ITwitchUnitStatus enemy in enemies) {
            if (enemy == null) {
                Debug.LogError("Null enemy found inside enemy room");
            }

            enemy.unitDeathEvent.AddListener(onEnemyKilled);
            if (spawnIn) {
                enemy.despawn();
            }
        }

        // Set up trigger room
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

        // Trigger reset event so that everything is appropriate by first frame
        resetEvent.Invoke();
    }

    // Main function to reset enemy group
    //  Pre: none, player dies and respawns to check point associated with this group
    //  Post: resets the room as if nothing has really happened
    public override void reset() {
        // reset enemies
        numEnemiesLeft = enemies.Length;
        foreach(ITwitchUnitStatus enemy in enemies) {
            if (spawnIn) {
                enemy.despawn();
            } else {
                enemy.reset();
            }
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
    //  Pre: an enemy belong to this enemy group has been killed
    //  Post: keeps track of how many enemies killed. If all enemies killed currently, do something
    public override void onEnemyKilled(IUnitStatus status) {

        // Add lock for secure synch
        lock(enemyLock) {
            numEnemiesLeft--;

            if (numEnemiesLeft <= 0) {
                allEnemiesDeadEvent.Invoke();
                PlayerCameraController.reset();
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
            if (spawnIn) {
                foreach(ITwitchUnitStatus enemy in enemies) {
                    enemy.spawnIn();
                }
            }

            // Move camera
            if (roomCamera != null) {
                PlayerCameraController.moveCamera(transform, roomCamera.transform.localPosition, roomCamera.transform.localRotation);
            }
        }
    }

}
