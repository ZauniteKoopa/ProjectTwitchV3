using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IBossScoutingBranch : MonoBehaviour
{
    // Main function to run the branch
    public abstract IEnumerator execute(Vector3 lastSuspectedPlayerPos, int phaseNumber);

    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public abstract void reset();

    // Main event handler for when enemy has been despawn because of player death and you must clean up all side effects
    //  By default, just reset
    public virtual void hardReset() {
        reset();
    }
}
