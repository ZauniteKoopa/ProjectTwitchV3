using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ITwitchInventory : MonoBehaviour
{
    // Main function to add an Ingredient to the current inventory
    //  Pre: ing != null 
    //  Post: Returns whether or not successful. If so, ingredient will be added in the inventory
    public abstract bool addIngredient(Ingredient ing);


    // Main function to remove ingredient from the current inventory
    //  Pre: ing != null
    //  Post: If removal is successful. If true, inventory removes 1 instance of ingredient from inventory
    public abstract bool removeIngredient(Ingredient ing);


    // Main accessor function to get the Primary Vial from this inventory
    //  Pre: none
    //  Post: Access a primary vial that CAN be null (means that you had an empty vial)
    public abstract IVial getPrimaryVial();


    // Main function to swap vials
    //  Pre: none 
    //  Post: primary vial will become secondary vial and vice versa
    public abstract void swapVials();


    // Main function to clear out inventory upon death
    //  Pre: None
    //  Post: Clears Inventory of everything
    public abstract void clear();


    // Main function to decrement primary vial count
    //  Pre: ammo >= 0
    //  Post: returns true if successful. If so, primary vial ammo count gets decremented
    public abstract bool consumePrimaryVial(int ammo);


    // Main function to upgrade primary vial with one ingredient
    //  Pre: Ing != null
    //  Post: Returns a bool that says if its successful. If so, primary vial gets upgraded
    public abstract bool upgradePrimaryVial(Ingredient ing);


    // Main function to upgrade primary vial with one ingredient
    //  Pre: Ing1 != null && ing2 != null
    //  Post: Returns a bool that says if its successful. If so, primary vial gets upgraded
    public abstract bool upgradePrimaryVial(Ingredient ing1, Ingredient ing2);


    // Main function to upgrade secondary vial with one ingredient
    //  Pre: Ing != null
    //  Post: Returns a bool that says if its successful. If so, secondary vial gets upgraded
    public abstract bool upgradeSecondaryVial(Ingredient ing);


    // Main function to upgrade primary vial with one ingredient
    //  Pre: Ing1 != null && ing2 != null
    //  Post: Returns a bool that says if its successful. If so, secondary vial gets upgraded
    public abstract bool upgradeSecondaryVial(Ingredient ing1, Ingredient ing2);
}
