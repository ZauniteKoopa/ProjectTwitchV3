using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ITwitchStatus : IUnitStatus
{
    // Main method to get access to primary poison vial
    //  Pre: none
    //  Post: returns the primary poison vial that player is using, CAN BE NULL
    public abstract IVial getPrimaryVial();


    // Main event handler function for swapping between 2 vials
    //  Pre: none
    //  Post: swaps primary and secondary vials on the fly
    public abstract void swapVials();


    // Main shorthand method to use primary vial
    //  Pre: ammoCost >= 0
    //  Post: returns if successful. If so, reduces primary vial's ammo
    public abstract bool usePrimaryVialAmmo(int ammoCost);
}
