using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Private instance variables
    [SerializeField]
    private IEnemyGroup[] enemyRooms;

    
    // If player dies, respawn
    public void respawnPlayer(Transform player) {
        player.position = transform.position;

        foreach(IEnemyGroup enemyRoom in enemyRooms) {
            enemyRoom.reset();
        }
    }
    


    // If you hit a player, set it to this checkpoint and disable collider (NOTE: ASSUMES A LINEAR LEVEL)
    private void OnTriggerEnter(Collider collider) {
        ITwitchStatus testPlayer = collider.GetComponent<ITwitchStatus>();

        if (testPlayer != null) {
            GetComponent<Collider>().enabled = false;
            testPlayer.setCheckpoint(this);
        }
    }
}
