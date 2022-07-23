using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    // Main override function for getting the description
    public override string getDescription() {
        return "Gain a new ultimate ability: throw a pulsating, slimy cask that latches on to anyone hit, slowing them down (scales with stickiness). After " + pullDelay + " seconds, the cask will pull all enemies towards the center. Costs " + ultCost + " ammo";
    }
}
