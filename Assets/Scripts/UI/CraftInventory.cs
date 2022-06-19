using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    // Inventory to display
    [SerializeField]
    private ITwitchInventory twitchInventory;

    // Meta variables to keep track of pause state
    private bool inInventory = false;
    private float prevTimeScale = 1.0f;


    // Function to update the current inventory with a new state
    //  Pre: None
    //  Post: Inventory UI has been updated with the latest version and change records are reset
    private void openInventory() {
        // Display basic ingredient icons and vial icons
        twitchInventory.displayIngredients(ingredients);
        primaryVialIcon.DisplayVial(twitchInventory.getPrimaryVial());
        twitchInventory.displaySecondaryVial(secondaryVialIcon);
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
    public void onCraftVialChange(IVial vial) {
        Debug.Log("event gotten");
        selectedVialInfo.DisplayVial(vial);
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
}
