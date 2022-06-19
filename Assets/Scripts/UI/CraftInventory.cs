using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class CraftInventory : MonoBehaviour
{
    // General materials
    [SerializeField]
    private IngredientIcon[] ingredients;
    [SerializeField]
    private VialInventoryIcon primaryVialIcon;
    [SerializeField]
    private VialInventoryIcon secondaryVialIcon;

    // Crafting
    [SerializeField]
    private CraftIngredientSlot craftIngSlot1;
    [SerializeField]
    private CraftIngredientSlot craftIngSlot2;
    [SerializeField]
    private CraftVialSlot craftVialSlot;

    // Information display
    [SerializeField]
    private VialIcon selectedVialInfo;
    [SerializeField]
    private IngredientDisplay ingredientInfo;

    // Inventory to display
    [SerializeField]
    private ITwitchInventory twitchInventory;

    // Meta variables to keep track of pause state
    private bool inInventory = false;
    private float prevTimeScale = 1.0f;


    // On awake, do additional setup and connect
    private void Awake() {
        // Connect all ingredient icons to onIngredientSelect event handler when ingredient is selected
        foreach (IngredientIcon ingIcon in ingredients) {
            ingIcon.OnIngredientSelect.AddListener(onIngredientSelect);
        }
    }


    // Function to update the current inventory with a new state
    //  Pre: None
    //  Post: Inventory UI has been updated with the latest version and change records are reset
    private void openInventory() {
        // Display basic ingredient icons and vial icons
        twitchInventory.displayIngredients(ingredients);
        primaryVialIcon.DisplayVial(twitchInventory.getPrimaryVial());
        twitchInventory.displaySecondaryVial(secondaryVialIcon);

        // Reset craft window
        craftIngSlot1.Reset();
        craftIngSlot2.Reset();
        craftVialSlot.Reset();

        // Reset information displays
        ingredientInfo.uiClear();
        selectedVialInfo.DisplayVial(null);
    }


    // Main event handler for when using the tab button
    //  Post: open or closes the inventory depending on the inInventory state
    public void onInventoryButtonPress() {
        // If you're not in inventory, open the inventory
        if (!inInventory) {
            inInventory = true;

            // Record time scale
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0.0f;

            // Open Inventory so that UI has latest version
            openInventory();

        // If you are in inventory, close inventory
        } else {
            inInventory = false;
            Time.timeScale = prevTimeScale;
        }

        gameObject.SetActive(inInventory);
    }


    // Main event handler function to handle when the selected vial changes
    //  Pre: none
    //  Post: selectedVial information will be updated with currently selected vial
    public void onCraftVialChange(IVial vial) {
        selectedVialInfo.DisplayVial(vial);
    }


    // Main event handler function for ingredient select
    public void onIngredientSelect(Ingredient ing) {

        // If ing is null, do a UI clear. Else, display ingredient
        if (ing != null) {
            ingredientInfo.displayIngredient(ing);
        } else {
            ingredientInfo.uiClear();
        }
    }


    // Main function to check if you are in paused state for this inventory
    public bool inventoryInterfaceActive() {
        return inInventory;
    }


    // Main function to handle inventory button press
    public void onInventoryKeyInputPress(InputAction.CallbackContext value) {
        if (value.started) {
            onInventoryButtonPress();
        }
    }


    // Main event handler function when the reset button is pressed
    public void onCraftResetButtonPress() {
        craftIngSlot1.Reset();
        craftIngSlot2.Reset();
        craftVialSlot.Reset();

        ingredientInfo.uiClear();
        selectedVialInfo.DisplayVial(null);
    }


    // Main event handler function when the craft button is pressed
    public void onCraftButtonPress() {
        Debug.Log("CRAFT!");
    }
}

