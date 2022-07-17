using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IBossScoutingBranch : MonoBehaviour
{
    // Main function to run the branch
    public abstract IEnumerator execute(Vector3 lastSuspectedPlayerPos, int phaseNumber);

    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public abstract void reset();
}
