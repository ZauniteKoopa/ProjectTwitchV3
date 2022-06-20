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

    private readonly object ingredientsLock = new object();
    private readonly object vialLock = new object();

    // UI Elements to signal (Modularize ITwitchPlayerUI? This is also pointed to by PlayerStatus)
    [Header("UI Elements")]
    [SerializeField]
    private ITwitchPlayerUI mainPlayerUI;


    // On awake, initialize backend instance variables
    private void Awake() {
        // Initialize variables
        primaryVial = new PoisonVial(2, 0, 2, 0, 40);
        secondaryVial = new PoisonVial(0, 2, 0, 2, 40);
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
            if (primaryVial == null) {
                success = false;
            } else {
                success = primaryVial.useVial(ammo);

                if (primaryVial.getAmmoLeft() <= 0) {
                    primaryVial = null;
                }
            }

            mainPlayerUI.displayPrimaryVial(primaryVial);
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

            // Display UI
            mainPlayerUI.displaySecondaryVial(secondaryVial);
            mainPlayerUI.displayPrimaryVial(primaryVial);
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

            // Display UI
            mainPlayerUI.displaySecondaryVial(secondaryVial);
            mainPlayerUI.displayPrimaryVial(primaryVial);
        }
    }


    // Main function to upgrade primary vial with one ingredient
    //  Pre: Ing != null
    //  Post: Returns a bool that says if its successful. If so, primary vial gets upgraded
    public override bool upgradePrimaryVial(Ingredient ing) {
        Debug.Assert(ing != null);
        bool success;

        lock (vialLock) {
            if (primaryVial == null) {
                primaryVial = new PoisonVial(ing);
                success = true;
            } else {
                success = primaryVial.upgrade(ing);
            }

            mainPlayerUI.displayPrimaryVial(primaryVial);
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
            if (primaryVial == null) {
                primaryVial = new PoisonVial(ing1, ing2);
                success = true;
            } else {
                success = primaryVial.upgrade(ing1, ing2);
            }

            mainPlayerUI.displayPrimaryVial(primaryVial);
        }

        return success;
    }


    // Main function to upgrade primary vial with one ingredient
    //  Pre: at least 1 of the ingredients is not null
    //  Post: Returns a bool that says if its successful. If so, primary vial gets replaced by new vial made by 2 ingredients
    public override bool replacePrimaryVial(Ingredient ing1, Ingredient ing2) {
        Debug.Assert(ing1 != null || ing2 != null);

        // If only 1 ingredient is non null, use the single constructor
        if (ing1 == null || ing2 == null) {
            Ingredient currentIng = (ing1 != null) ? ing1 : ing2;
            primaryVial = new PoisonVial(currentIng);
        } else {
            primaryVial = new PoisonVial(ing1, ing2);
        }

        return true;
    }


    // Main function to upgrade secondary vial with one ingredient
    //  Pre: Ing != null
    //  Post: Returns a bool that says if its successful. If so, secondary vial gets upgraded
    public override bool upgradeSecondaryVial(Ingredient ing) {
        Debug.Assert(ing != null);
        bool success;

        lock (vialLock) {
            if (secondaryVial == null) {
                secondaryVial = new PoisonVial(ing);
                success = true;
            } else {
                success = secondaryVial.upgrade(ing);
            }

            mainPlayerUI.displaySecondaryVial(secondaryVial);
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
            if (secondaryVial == null) {
                secondaryVial = new PoisonVial(ing1, ing2);
                success = true;
            } else {
                success = secondaryVial.upgrade(ing1, ing2);
            }

            mainPlayerUI.displaySecondaryVial(secondaryVial);
        }

        return success;
    }


    // Main function to upgrade secondary vial with one ingredient
    //  Pre: at least 1 of the ingredients is not null
    //  Post: Returns a bool that says if its successful. If so, secondary vial gets replaced by new vial made by 2 ingredients
    public override bool replaceSecondaryVial(Ingredient ing1, Ingredient ing2) {
        Debug.Assert(ing1 != null || ing2 != null);

        // If only 1 ingredient is non null, use the single constructor
        if (ing1 == null || ing2 == null) {
            Ingredient currentIng = (ing1 != null) ? ing1 : ing2;
            secondaryVial = new PoisonVial(currentIng);
        } else {
            secondaryVial = new PoisonVial(ing1, ing2);
        }

        return true;
    }


    // Main function to display ingredients given an array of ingredient icons
    //  Pre: array of icons != null with non-null elements AND array length >= number of distinct ingredient types
    //  Post: Displays ingredients onto ingredient icons
    public override void displayIngredients(IngredientIcon[] ingredientIcons) {
        // Pre condition
        Debug.Assert(ingredientIcons != null && ingredientIcons.Length >= ingredientInventory.Count);

        // Index used to access the icons array
        int iconIndex = 0;

        // Iterate over each entry within the dictionary
        foreach (KeyValuePair<Ingredient, int> entry in ingredientInventory) {
            // Set up each ingredient icon
            IngredientIcon currentIcon = ingredientIcons[iconIndex];
            Debug.Assert(currentIcon != null);
            currentIcon.SetUpIcon(entry.Key, entry.Value);

            // Increment iconIndex
            iconIndex++;
        }

        // You end on the last iconIndex that's empty, clear out all remaining icons
        for (int i = iconIndex; i < ingredientIcons.Length; i++) {
            IngredientIcon currentIcon = ingredientIcons[i];
            Debug.Assert(currentIcon != null);
            currentIcon.ClearIcon();
        }
    }


    // Main function to display secondary vial in an icon
    //  Pre: VialIcon must not be null
    //  Post: VialIcon will now display secondaryVial
    public override void displaySecondaryVial(VialIcon vialIcon) {
        vialIcon.DisplayVial(secondaryVial);
    }


    // Main function to check if you can upgrade a vial
    //  Pre: Ing1 and Ing2 are ingredients used to upgrade (can be null) and isPrimary is a bool: true - upgrade primary, false - upgrade secondary
    //  Post: returns a bool to check if you can really upgrade, if vial is null, returns true immediately
    public override bool canUpgradeVial(Ingredient ing1, Ingredient ing2, bool isPrimary) {
        // Get the target vial
        IVial targetVial = (isPrimary) ? primaryVial : secondaryVial;
        
        // If null, return true immediately
        if (targetVial == null) {
            return true;
        }

        // Calculate stat contributions and see if it's still less than targetVial.getMaxTotalStat
        int ingContribution1 = (ing1 == null) ? 0 : ing1.getNumStatContribution();
        int ingContribution2 = (ing2 == null) ? 0 : ing2.getNumStatContribution();
        
        return ingContribution1 + ingContribution2 <= targetVial.getMaxTotalStat();
    }
}
