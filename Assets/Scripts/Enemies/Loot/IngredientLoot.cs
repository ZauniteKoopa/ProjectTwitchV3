using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientLoot : Loot
{
    [SerializeField]
    private string ingredientName;
    private Ingredient ingInstance;

    // On start, change the color of loot to ingredient color
    private void Start() {
        ingInstance = IngredientDatabase.getIngredient(ingredientName);
        if (ingInstance == null) {
            Debug.LogError("INGREDIENT NAME TYPO AAAAAAAAA");
        }

        MeshRenderer meshRender = GetComponent<MeshRenderer>();
        meshRender.material.color = ingInstance.getColor();
    }

    // Main function to interact with ingredient
    public override bool onPlayerCollect (ITwitchInventory inventory) {

        bool success = inventory.addIngredient(ingInstance);
        if (success) {
            gameObject.SetActive(false);
        }

        return success;
    }


    // Main function to quick craft with player 
    public override bool onPlayerQuickCraft(ITwitchInventory inventory, bool isPrimary) {

        bool success = (isPrimary) ? inventory.upgradePrimaryVial(ingInstance, false) : inventory.upgradeSecondaryVial(ingInstance, false);
        if (success) {
            gameObject.SetActive(false);
        }

        return success;
    }


    // Main function to access ingredient
    public override Ingredient getIngredient() {
        return ingInstance;
    }
}
