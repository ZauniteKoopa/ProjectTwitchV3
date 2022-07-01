using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ITwitchStatus : ITwitchUnitStatus
{
    public UnityEvent contaminateUsedEvent;
    public UnityEvent contaminateReadyEvent;

    // Main method to get access to primary poison vial
    //  Pre: none
    //  Post: returns the primary poison vial that player is using, CAN BE NULL
    public abstract IVial getPrimaryVial();


    // Main event handler function for swapping between 2 vials
    //  Pre: none
    //  Post: swaps primary and secondary vials on the fly
    public abstract void swapVials();

    
    // Main function to use bolt bullet with poison vial wth defined cost
    //  Pre: none
    //  Post: returns if successful, If so, reduce primary vial's ammo
    public abstract bool consumePrimaryVialBullet();


    // Main function to use cask bullet wth defined cost
    //  Pre: none
    //  Post: returns if successful, If so, reduce primary vial's ammo
    public abstract bool consumePrimaryVialCask();


    // Main function to get permissions to cast contaminate
    //  Pre: none
    //  Post: return if you are allowed. If successful, must wait for cooldown to stop to do it again
    public abstract bool willContaminate();


    // Main function to get permissions to cast camofladge
    //  Pre: none
    //  Post: return if you are allowed. If successful, must wait for sequence to end to do it again
    public abstract bool willCamofladge();


    // Main function to get attack rate effect factor
    //  Pre: none
    //  Post: returns a variable > 0.0f;
    public abstract float getAttackRateFactor();


    // Function to set checkpoint
    //  Pre: checkpoint != null
    //  Post: character will now be assigned to this checkpoint
    public abstract void setCheckpoint(Checkpoint cp);


    // Function to see if you can see the player is visible to enemy
    //  Pre: enemy != null
    //  Post: returns whether the enemy can see the player. Does not consider distance or walls inbetween
    public abstract bool isVisible(Collider enemy);


    // Event handler for when stealth has been reset.
    //  Pre: Stealth resets IFF the player has killed at least one unit with expunge
    //  Post: stealth cooldown will reset. HOWEVER, cannot use camofladge when stealth sequence already running
    public abstract void onStealthReset();
}
