using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInterfaceInputModule : MonoBehaviour
{
    [SerializeField]
    private CraftInventory inventoryUI;
    [SerializeField]
    private IPauseMenu pauseMenu;
    [SerializeField]
    private IUnitStatus playerStatus;


    // On awake, error check
    private void Awake() {
        if (inventoryUI == null) {
            Debug.LogError("User Interface Input Module not connected to player's inventory UI");
        }

        if (pauseMenu == null) {
            Debug.LogError("User Interface Input Module not connected to player's pause menu");
        }

        if (playerStatus == null) {
            Debug.LogError("User Interface Input Module not connected to player status");
        }
    }


    // Main function to check whether or not the UI is in Menu
    //  Pre: none
    //  Post: Checks if player is in menu, disables all controls if so
    public bool inMenu() {
        return pauseMenu.inPauseState() || inventoryUI.inventoryInterfaceActive();
    }


    // Main event handler function for when pause input sensed on keyboard / controller
    //  Pre: InputSystem sensed Input context for assigned input mapping
    //  Post: Pauses or unpauses menu
    public void onPauseKeyInputPress(InputAction.CallbackContext value) {
        if (value.started && !inventoryUI.inventoryInterfaceActive() && playerStatus.isAlive()) {
            pauseMenu.onPauseButtonPress();
        }
    }


    // Main function to handle inventory button press
    public void onInventoryKeyInputPress(InputAction.CallbackContext value) {
        if (value.started && !pauseMenu.inPauseState() && playerStatus.isAlive()) {
            inventoryUI.onInventoryButtonPress();
        }
    }

}
