using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class IPauseMenu : MonoBehaviour
{
    // Main event handler function for when the pause button is pressed
    //  Pre: none
    //  Post: Handles for when either pause key is pressed, or when unpause UI button pressed
    public abstract void onPauseButtonPress();


    // Main event handler function for exiting the game
    //  Pre: none
    //  Post: will exit the application IFF application is paused on this menu
    public abstract void onExitApplication();


    // Main event handler function for when pause input sensed on keyboard / controller
    //  Pre: InputSystem sensed Input context for assigned input mapping
    //  Post: Pauses or unpauses menu
    public abstract void onPauseKeyInputPress(InputAction.CallbackContext value);


    // Accessor function to check if you're in the pause state
    //  Pre: None
    //  Post: returns whether you're in pause state
    public abstract bool inPauseState();
}
