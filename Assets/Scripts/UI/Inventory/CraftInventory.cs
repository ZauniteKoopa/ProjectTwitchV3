using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class CraftInventory : MonoBehaviour
{
    // General materials
    [Header("Inventory Elements")]
    [SerializeField]
    private IngredientIcon[] ingredients;
    [SerializeField]
    private VialInventoryIcon primaryVialIcon;
    [SerializeField]
    private VialInventoryIcon secondaryVialIcon;
    private bool primaryVialSelected = true;

    // Crafting
    [Header("Crafting Window")]
    [SerializeField]
    private CraftIngredientSlot craftIngSlot1;
    [SerializeField]
    private CraftIngredientSlot craftIngSlot2;
    [SerializeField]
    private CraftVialSlot craftVialSlot;
    [SerializeField]
    private TMP_Text craftErrorMessage;

    // Information display
    [Header("Information Display")]
    [SerializeField]
    private VialIcon selectedVialInfo;
    [SerializeField]
    private IngredientDisplay ingredientInfo;

    // Inventory to display
    [Header("Backend Inventory")]
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

        // Connect to Unity Events for vial inventory icon
        primaryVialIcon.iconSelectedEvent.AddListener(onPrimaryVialSelect);
        secondaryVialIcon.iconSelectedEvent.AddListener(onSecondaryVialSelect);
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
        craftErrorMessage.gameObject.SetActive(false);

        // Reset information displays
        ingredientInfo.uiClear();

        // Display primary vial
        primaryVialIcon.setHighlight(true);
        secondaryVialIcon.setHighlight(false);
        selectedVialInfo.DisplayVial(twitchInventory.getPrimaryVial());
        primaryVialSelected = true;
    }


    // Private instance function to close Inventory UI
    //  Pre: closes inventory UI and resumes game
    //  Post: finishes UI
    private void closeInventory() {
        inInventory = false;
        Time.timeScale = prevTimeScale;
        gameObject.SetActive(false);
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
            gameObject.SetActive(true);
            openInventory();

        // If you are in inventory, close inventory
        } else {
            closeInventory();
        }
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


    // Main event handler function for when the primary vial icon is selected
    //  If not previously selected, set flag to true and have all vial information display primary vial
    public void onPrimaryVialSelect() {
        if (!primaryVialSelected) {
            // Update flags and information
            primaryVialSelected = true;
            craftVialSlot.Reset();
            selectedVialInfo.DisplayVial(twitchInventory.getPrimaryVial());

            // Change highlights
            primaryVialIcon.setHighlight(true);
            secondaryVialIcon.setHighlight(false);
        }
    }


    // Main event handler function for when the secondary vial icon is selected
    //  If not previously selected, set flag to false and have all vial info display secondary vial
    public void onSecondaryVialSelect() {
        if (primaryVialSelected) {
            // Update flags and info
            primaryVialSelected = false;
            craftVialSlot.Reset();
            twitchInventory.displaySecondaryVial(selectedVialInfo);

            // Update highlights
            primaryVialIcon.setHighlight(false);
            secondaryVialIcon.setHighlight(true);
        }
    }


    // Main function to check if you are in paused state for this inventory
    public bool inventoryInterfaceActive() {
        return inInventory;
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
        // Check for errors: no ingredients found
        Ingredient ing1 = craftIngSlot1.GetIngredient();
        Ingredient ing2 = craftIngSlot2.GetIngredient();

        if (ing1 == null && ing2 == null) {
            craftErrorMessage.gameObject.SetActive(true);
            craftErrorMessage.text = "No Ingredients found in crafting table";
            return;
        }


        // Check for errors: ingredient contribution goes over max stat cap
        bool canUpgrade = craftVialSlot.GetVial() == null || twitchInventory.canUpgradeVial(ing1, ing2, primaryVialSelected);

        if (canUpgrade) {
            craftErrorMessage.gameObject.SetActive(false);
            inventoryCraft(ing1, ing2);
        } else {
            craftErrorMessage.gameObject.SetActive(true);
            craftErrorMessage.text = "Will reach max stat cap: cannot upgrade stat";
            return;
        }
    }


    // Private helper function to do inventory crafting
    //  Pre: At least 1 of the ingredients is non-null
    //  Post: upgrades vial depending on inventory
    private void inventoryCraft(Ingredient ing1, Ingredient ing2) {
        Debug.Assert(ing1 != null || ing2 != null);

        // Will you replace? (No craft vial in the slot)
        bool willReplace = craftVialSlot.GetVial() == null;

        // Case where you're replacing a vial
        if (willReplace) {

            // Replace vial depending on which is selected
            if (primaryVialSelected) {
                twitchInventory.replacePrimaryVial(ing1, ing2);
            } else {
                twitchInventory.replaceSecondaryVial(ing1, ing2);
            }

        // Case where you're upgrading a vial
        } else {
            
            // Only 1 ingredient available
            if (ing1 == null || ing2 == null) {
                Ingredient currentIngredient = (ing1 != null) ? ing1 : ing2;

                if (primaryVialSelected) {
                    twitchInventory.upgradePrimaryVial(currentIngredient, true);
                } else {
                    twitchInventory.upgradeSecondaryVial(currentIngredient, true);
                }

            // When both ingredients are available
            } else {
                if (primaryVialSelected) {
                    twitchInventory.upgradePrimaryVial(ing1, ing2);
                } else {
                    twitchInventory.upgradeSecondaryVial(ing1, ing2);
                }
            }
        }

        // Update infromation after craft and consume elements
        if (ing1 != null) {
            twitchInventory.removeIngredient(ing1);
        }

        if (ing2 != null) {
            twitchInventory.removeIngredient(ing2);
        }

        craftIngSlot1.CraftIngredient();
        craftIngSlot2.CraftIngredient();
        craftVialSlot.Reset();

        updateInfo();
        closeInventory();
    }


    // Private helper function to update infromation to get the most up-to-date version
    private void updateInfo() {
        // Display basic ingredient icons and vial icons
        twitchInventory.displayIngredients(ingredients);
        primaryVialIcon.DisplayVial(twitchInventory.getPrimaryVial());
        twitchInventory.displaySecondaryVial(secondaryVialIcon);

        // Reset information displays
        ingredientInfo.uiClear();

        // Update vial display
        if (primaryVialSelected) {
            selectedVialInfo.DisplayVial(twitchInventory.getPrimaryVial());
        } else {
            twitchInventory.displaySecondaryVial(selectedVialInfo);
        }
    }
}

