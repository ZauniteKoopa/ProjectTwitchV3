using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TwitchInventory : ITwitchInventory
{
    // Private instance variables
    private IVial primaryVial;
    private IVial secondaryVial;
    private Dictionary<Ingredient, int> ingredientInventory;

    private readonly object ingredientsLock;
    private readonly object vialLock;

    // UI Elements to signal (Modularize ITwitchPlayerUI? This is also pointed to by PlayerStatus)
    [Header("UI Elements")]
    [SerializeField]
    private ITwitchPlayerUI mainPlayerUI;


    // On awake, initialize backend instance variables
    private void Awake() {
        // Initialize variables
        primaryVial = new PoisonVial(3, 0, 2, 0, 40);
        secondaryVial = new PoisonVial(0, 2, 0, 3, 40);
        ingredientInventory = new Dictionary<Ingredient, int>();

        // Display UI
        mainPlayerUI.displaySecondaryVial(secondaryVial);
        mainPlayerUI.displayPrimaryVial(primaryVial);
    }



    // Main function to add an Ingredient to the current inventory
    //  Pre: ing != null 
    //  Post: Returns whether or not successful. If so, ingredient will be added in the inventory
    public override bool addIngredient(Ingredient ing) {
        Debug.Assert(ing != null);

        lock(ingredientsLock) {
            // If not found in Inventory, add it with a number of 0
            if (!ingredientInventory.ContainsKey(ing)) {
                ingredientInventory.Add(ing, 0);
            }

            // Increment count
            ingredientInventory[ing]++;
        }

        return true;
    }


    // Main function to remove ingredient from the current inventory
    //  Pre: ing != null
    //  Post: If removal is successful. If true, inventory removes 1 instance of ingredient from inventory
    public override bool removeIngredient(Ingredient ing) {
        Debug.Assert(ing != null);
        bool removalSuccess;

        lock (ingredientsLock) {
            // Successful removal is dependent on whether or not you have at least 1 instance in inventory
            removalSuccess = ingredientInventory.ContainsKey(ing);

            // If ingredient found in inventory, decrement
            if (removalSuccess) {
                ingredientInventory[ing]--;

                // If you have no more instances of ingredient anymore, remove it
                if (ingredientInventory[ing] <= 0) {
                    ingredientInventory.Remove(ing);
                }
            }
        }

        return removalSuccess;
    }


    // Main accessor function to get the Primary Vial from this inventory
    //  Pre: none
    //  Post: Access a primary vial that CAN be null (means that you had an empty vial)
    public override IVial getPrimaryVial() {
        return primaryVial;
    }


    // Main function to decrement primary vial count
    //  Pre: ammo >= 0
    //  Post: returns true if successful. If so, primary vial ammo count gets decremented
    public override bool consumePrimaryVial(int ammo) {
        Debug.Assert(ammo >= 0);
        bool success;

        lock (vialLock) {
            success = primaryVial.useVial(ammo);
        }

        return success;
    }


    // Main function to swap vials
    //  Pre: none 
    //  Post: primary vial will become secondary vial and vice versa
    public override void swapVials() {
        lock (vialLock) {
            IVial tempVial = primaryVial;
            primaryVial = secondaryVial;
            secondaryVial = tempVial;
        }
    }


    // Main function to clear out inventory upon death
    //  Pre: None
    //  Post: Clears Inventory of everything
    public override void clear() {
        // Clear Ingredient inventory
        lock (ingredientsLock) {
            ingredientInventory.Clear();
        }

        // Clear poison vials
        lock (vialLock) {
            primaryVial = null;
            secondaryVial = null;
        }
    }


    // Main function to upgrade primary vial with one ingredient
    //  Pre: Ing != null
    //  Post: Returns a bool that says if its successful. If so, primary vial gets upgraded
    public override bool upgradePrimaryVial(Ingredient ing) {
        Debug.Assert(ing != null);
        bool success;

        lock (vialLock) {
            success = primaryVial.upgrade(ing);
        }

        return success;
    }


    // Main function to upgrade primary vial with one ingredient
    //  Pre: Ing1 != null && ing2 != null
    //  Post: Returns a bool that says if its successful. If so, primary vial gets upgraded
    public override bool upgradePrimaryVial(Ingredient ing1, Ingredient ing2) {
        Debug.Assert(ing1 != null && ing2 != null);
        bool success;

        lock (vialLock) {
            success = primaryVial.upgrade(ing1, ing2);
        }

        return success;
    }


    // Main function to upgrade secondary vial with one ingredient
    //  Pre: Ing != null
    //  Post: Returns a bool that says if its successful. If so, secondary vial gets upgraded
    public override bool upgradeSecondaryVial(Ingredient ing) {
        Debug.Assert(ing != null);
        bool success;

        lock (vialLock) {
            success = secondaryVial.upgrade(ing);
        }

        return success;
    }


    // Main function to upgrade primary vial with one ingredient
    //  Pre: Ing1 != null && ing2 != null
    //  Post: Returns a bool that says if its successful. If so, secondary vial gets upgraded
    public override bool upgradeSecondaryVial(Ingredient ing1, Ingredient ing2) {
        Debug.Assert(ing1 != null && ing2 != null);
        bool success;

        lock (vialLock) {
            success = secondaryVial.upgrade(ing1, ing2);
        }

        return success;
    }
}
