using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    //  Pre: Ing != null, inMenu indicated whether or not you are doing crafting in inventory
    //  Post: Returns a bool that says if its successful. If so, primary vial gets upgraded
    public abstract bool upgradePrimaryVial(Ingredient ing, bool inMenu);


    // Main function to upgrade primary vial with two ingredients
    //  Pre: Ing1 != null && ing2 != null
    //  Post: Returns a bool that says if its successful. If so, primary vial gets upgraded
    public abstract bool upgradePrimaryVial(Ingredient ing1, Ingredient ing2);


    // Main function to upgrade primary vial with one ingredient
    //  Pre: at least 1 of the ingredients is not null
    //  Post: Returns a bool that says if its successful. If so, primary vial gets replaced by new vial made by 2 ingredients
    public abstract bool replacePrimaryVial(Ingredient ing1, Ingredient ing2);


    // Main function to upgrade secondary vial with one ingredient
    //  Pre: Ing != null, inMenu indicated whether or not you are doing crafting in inventory
    //  Post: Returns a bool that says if its successful. If so, secondary vial gets upgraded
    public abstract bool upgradeSecondaryVial(Ingredient ing, bool inMenu);


    // Main function to upgrade secondary vial with two ingredient
    //  Pre: Ing1 != null && ing2 != null
    //  Post: Returns a bool that says if its successful. If so, secondary vial gets upgraded
    public abstract bool upgradeSecondaryVial(Ingredient ing1, Ingredient ing2);


    // Main function to upgrade secondary vial with one ingredient
    //  Pre: at least 1 of the ingredients is not null
    //  Post: Returns a bool that says if its successful. If so, secondary vial gets replaced by new vial made by 2 ingredients
    public abstract bool replaceSecondaryVial(Ingredient ing1, Ingredient ing2);


    // Main function to display ingredients given an array of ingredient icons
    //  Pre: array of icons != null with non-null elements AND array length >= number of distinct ingredient types
    //  Post: Displays ingredients onto ingredient icons
    public abstract void displayIngredients(IngredientIcon[] ingredientIcons);


    // Main function to display secondary vial in an icon
    //  Pre: VialIcon must not be null
    //  Post: VialIcon will now display secondaryVial
    public abstract void displaySecondaryVial(VialIcon vialIcon);


    // Main function to check if you can upgrade a vial
    //  Pre: Ing1 and Ing2 are ingredients used to upgrade (can be null) and isPrimary is a bool: true - upgrade primary, false - upgrade secondary
    //  Post: returns a bool to check if you can really upgrade, if vial is null, returns true immediately
    public abstract bool canUpgradeVial(Ingredient ing1, Ingredient ing2, bool isPrimary);


    // Main function to add craft event listener
    //  Pre: eventHandler does not equal null
    //  Post: eventHandler will listen to craftEvent of inventory. Will also initialize event if it has not been initialized yet
    public abstract void addCraftListener(UnityAction eventHandler);


    // Main function to run craftingSequence
    //  Pre: float to determin how long the crafting lasts > 0, NO CONCONCURRENT CRAFTING
    //  Post: runs crafting sequence
    public abstract IEnumerator craftSequence(float craftTime);
}
