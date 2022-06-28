using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IEnemyPassiveBranch : MonoBehaviour
{
    // Main function to run the branch
    public abstract IEnumerator execute();

    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public abstract void reset();
}
