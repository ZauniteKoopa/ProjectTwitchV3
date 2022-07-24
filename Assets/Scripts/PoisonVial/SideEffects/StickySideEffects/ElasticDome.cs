using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ElasticDome : VirtualSideEffect
{
    [SerializeField]
    private float ultCooldown = 15f;
    [SerializeField]
    private int ultCost = 5;
    [SerializeField]
    private float elasticStunDuration = 0.75f;
    [SerializeField]
    private float baseDomeDuration = 2.5f;
    [SerializeField]
    private float domeDurationGrowth = 1f;
    [SerializeField]
    private LobbingUltimate elasticDomePrefab;


    // Main function to check if this is an ultimate
    //  Pre: none
    //  Post: returns the ultimate type for Twitch's Juice. If return NONE, cannot use this as an ultimate
    public override UltimateType getUltType() {
        return UltimateType.LOB;
    }


    // Main function to get ultimate cooldown
    //  Pre: none
    //  Post: cooldown > 0f
    public override float getUltimateCooldown() {
        return ultCooldown;
    }

    // Main function to get ultimate ammo cost
    //  Pre: none
    //  Post: cost > 0f
    public override int getUltimateCost() {
        return ultCost;
    }


    // Main function to throw ult lobs
    //  Pre: startPosition is the start position of the lob, end position is the end position of the lob, statNum is the important stat value
    //  Post: launches lobbing ultimate
    public override void throwLobbingUltimate(Vector3 startPos, Vector3 endPos, int statNum) {
        // Calculate values
        float currentDuration = Mathf.Max(baseDomeDuration + (domeDurationGrowth * (statNum - 3)), baseDomeDuration);
        float[] ultParameters = new float[] {currentDuration, elasticStunDuration};

        // Instantiate objects
        LobbingUltimate currentUlt = Object.Instantiate(elasticDomePrefab, startPos, Quaternion.identity);
        currentUlt.launch(endPos, ultParameters);
    }


    // Main override function for getting the description
    public override string getDescription() {
        return "New ultimate: Throw a cask that forms an elastic dome that blocks all enemy attacks. All enemies that enter or exit the dome will be stunned for " + elasticStunDuration + " seconds. Dome lifespan scales with stickiness";
    }
}
