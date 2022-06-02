using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IEnemyAggroBranch : MonoBehaviour
{
    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public abstract IEnumerator execute(Transform tgt);


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public abstract IEnumerator reset();
}
