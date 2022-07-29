using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu]
public class TwitchJuice : VirtualSideEffect
{
    [SerializeField]
    private float healthPercentHealed = 0.25f;
    [SerializeField]
    private float healTime = 6f;
    [SerializeField]
    private float ultCooldown = 15f;
    [SerializeField]
    private int ultCost = 5;


    // Main function to check if this is an ultimate
    //  Pre: none
    //  Post: returns the ultimate type for Twitch's Juice. If return NONE, cannot use this as an ultimate
    public override UltimateType getUltType() {
        return UltimateType.STEROID;
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


    // Main function to apply steroid to player character
    //  Pre: playerStatus != null
    //  Post: Applies status effect on player
    public override void applySteroid(ITwitchStatus player) {
        Debug.Assert(player != null);

        player.applyHealthRegenEffect(healthPercentHealed, healTime);
    }


    // Main override function for getting the description
    public override string getDescription() {
        float displayHealthPercent = healthPercentHealed * 100f;
        return "Gain a new ultimate ability to drink the poison to heal " + displayHealthPercent + "% of max health gradually in the span of " + healTime + " seconds. Costs " + ultCost + " ammo";
    }
}
