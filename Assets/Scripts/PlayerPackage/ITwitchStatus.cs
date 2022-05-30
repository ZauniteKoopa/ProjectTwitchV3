using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ITwitchStatus : ITwitchUnitStatus
{
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
    //  Post: return if you are allowed. If successful, must wait for cooldown to stop to do it again
    public abstract bool willCamofladge();
}
