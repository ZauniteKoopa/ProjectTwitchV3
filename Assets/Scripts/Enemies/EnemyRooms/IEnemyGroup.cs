using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IEnemyGroup : MonoBehaviour
{
    // Main function to reset enemy group
    //  Pre: none, player dies and respawns to check point associated with this group
    //  Post: resets the room as if nothing has really happened
    public abstract void reset();


    // Event handler function to handle when an enemy belong to this room has been killed
    //  Pre: an enemy belong to this enemy group has been killed
    //  Post: keeps track of how many enemies killed. If all enemies killed currently, do something
    public abstract void onEnemyKilled();
}
