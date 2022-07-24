using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PulsatingCask : VirtualSideEffect
{
    [SerializeField]
    private float ultCooldown = 15f;
    [SerializeField]
    private int ultCost = 5;
    [SerializeField]
    private float baseSlow = 0.6f;
    [SerializeField]
    private float slowGrowth = -0.1f;
    [SerializeField]
    private float pullDistance = 4f;
    [SerializeField]
    private float pullDelay = 1f;
    [SerializeField]
    private LobbingUltimate pulsatingCaskPrefab;


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
        float currentCaskSlow = Mathf.Min(baseSlow, baseSlow + (slowGrowth * (statNum - 3)));
        float[] ultParameters = new float[] {currentCaskSlow, pullDistance, pullDelay};

        // Instantiate objects
        LobbingUltimate currentUlt = Object.Instantiate(pulsatingCaskPrefab, startPos, Quaternion.identity);
        currentUlt.launch(endPos, ultParameters);
    }


    // Main override function for getting the description
    public override string getDescription() {
        return "Gain a new ultimate ability: throw a pulsating, slimy cask that latches on to anyone hit, slowing them down (scales with stickiness). After " + pullDelay + " seconds, the cask will pull all enemies towards the center. Costs " + ultCost + " ammo";
    }
}
