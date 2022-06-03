using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrushbotAggroBranch : IEnemyAggroBranch
{
    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt) {
        // Just face the player (to be changed)
        yield return 0;
        transform.forward = (tgt.position - transform.position).normalized;
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {}
}
