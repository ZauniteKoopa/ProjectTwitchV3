using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleColorBossScoutBranch : IBossScoutingBranch
{
    // Main function to run the branch
    public override IEnumerator execute(Vector3 lastSuspectedPlayerPos, int phaseNumber) {
        GetComponent<MeshRenderer>().material.color = Color.yellow;
        yield return 0;
    }

    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {}
}
