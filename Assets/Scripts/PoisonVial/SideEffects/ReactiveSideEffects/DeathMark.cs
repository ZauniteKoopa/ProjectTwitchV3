using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DeathMark : VirtualSideEffect
{
    [SerializeField]
    private float normalEnemyDeathThreshold = 0.3f;
    [SerializeField]
    private float bossDeathThreshold = 0.15f;
    [SerializeField]
    private int stackReq = 6;


    // Main boolean to check if you can execute with this vial
    //  Pre: isBoss indicates whether this is a boss or not, 0.0f <= healthPercentRemaining <= 1.0, 0 <= numStacks <= 6
    //  Post: returns a boolean whether or not this enemy can get immediately executed
    public override bool canExecute(bool isBoss, float healthPercentRemaining, int numStacks) {
        if (numStacks >= stackReq) {
            float curDeathThreshold = (isBoss) ? bossDeathThreshold : normalEnemyDeathThreshold;
            return healthPercentRemaining <= curDeathThreshold;
        }

        return false;
    }


    // Main override function for getting the description
    public override string getDescription() {
        float normalEnemyPercent = normalEnemyDeathThreshold * 100f;
        float bossEnemyPercent = bossDeathThreshold * 100f;
        return "When an enemy hits " + stackReq + " stacks, they are marked for DEATH. When an enemy reaches below " + normalEnemyPercent + "% health, they will be executed immediately. For bosses, " + bossEnemyPercent + "% health. Executions reset stealth";
    }

}
