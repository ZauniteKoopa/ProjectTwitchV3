using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TwitchJuice : VirtualSideEffect
{
    private static float healthPercentHealed = 0.25f;
    private static float healTime = 6f;
    private static float ultCooldown = 12f;
    private static int ultCost = 5;

    // Default constructor
    public TwitchJuice() : base (
        "Twitch's Juice",
        "Gain an Ultimate: When pressing R, drink the vial: heals " + (healthPercentHealed * 100f) + "% health gradually over " + healTime + " seconds. Cost " + ultCost + "ammo",
        Specialization.POISON
    ) {}


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

        // Calculate health gain per frame
        float healingAmount = player.getMaxHealth() * healthPercentHealed;
        float numTicks = healTime / Time.fixedDeltaTime;
        float healPerFrame = healingAmount / numTicks;

        player.applyHealthRegenEffect(healPerFrame, healTime);
    }
}
