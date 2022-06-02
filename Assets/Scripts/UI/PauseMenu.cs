using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Pause menu class: MUST BE CONNECTED TO THE PAUSE MENU GAME OBJECT TO BE USED
public class PauseMenu : IPauseMenu
{
    // Private helper functions
    private bool paused = false;
    private float prevTimeScale = 1.0f;


    // Main event handler function for when the pause button is pressed
    //  Pre: none
    //  Post: Handles for when either pause key is pressed, or when unpause UI button pressed
    public override void onPauseButtonPress() {

        // If game is not in pause menu state, go into pause menu state (keeping track of previous time scale)
        if (!paused) {
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0.0f;
            paused = true;

        // If game is already in pause menu state, get out of pause menu state
        } else {
            Time.timeScale = prevTimeScale;
            paused = false;
        }

        gameObject.SetActive(paused);
    }


    // Main event handler function for exiting the game
    //  Pre: none
    //  Post: will exit the application IFF application is paused on this menu
    public override void onExitApplication() {
        if (paused) {
            Application.Quit();
        }
    }


    // Main event handler function for when pause input sensed on keyboard / controller
    //  Pre: InputSystem sensed Input context for assigned input mapping
    //  Post: Pauses or unpauses menu
    public override void onPauseKeyInputPress(InputAction.CallbackContext value) {
        if (value.started) {
            onPauseButtonPress();
        }
    }


    // Main function to check if you're in pause state
    public override bool inPauseState() {
        return paused;
    }
}
