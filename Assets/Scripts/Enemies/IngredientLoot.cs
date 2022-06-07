using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientLoot : Loot
{
    [SerializeField]
    private string ingredientName;

    // Main function to interact with ingredient
    public override bool onPlayerCollect (ITwitchInventory inventory) {
        Ingredient ingInstance = IngredientDatabase.getIngredient(ingredientName);

        if (ingInstance == null) {
            Debug.LogError("INGREDIENT NAME TYPO AAAAAAAAA");
        }

        bool success = inventory.addIngredient(ingInstance);
        if (success) {
            gameObject.SetActive(false);
        }

        return success;
    }


    // Main function to quick craft with player 
    public override bool onPlayerQuickCraft(ITwitchInventory inventory, bool isPrimary) {
        Ingredient ingInstance = IngredientDatabase.getIngredient(ingredientName);

        if (ingInstance == null) {
            Debug.LogError("INGREDIENT NAME TYPO AAAAAAAAA");
        }

        bool success = (isPrimary) ? inventory.upgradePrimaryVial(ingInstance) : inventory.upgradeSecondaryVial(ingInstance);
        if (success) {
            gameObject.SetActive(false);
        }

        return success;
    }
}
