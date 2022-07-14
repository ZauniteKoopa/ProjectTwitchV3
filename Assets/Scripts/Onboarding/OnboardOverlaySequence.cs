using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;

public class OnboardOverlaySequence : MonoBehaviour
{
    // Private instance variables
    private PlayerInput mainPlayerInput;
    [SerializeField]
    private GameObject[] overlayDisplays;
    private int currentPage = -1;
    private float prevTimeScale = 1f;


    // On awake, find the player's default mainPlayerInput and start the first action
    private void Awake() {
        // Find player status
        TopDownMovementController3D playerController = FindObjectOfType<TopDownMovementController3D>();
        mainPlayerInput = playerController.GetComponent<PlayerInput>();

        // Disable all popups in overlay sequence
        foreach (GameObject popup in overlayDisplays) {
            popup.SetActive(false);
        }

        // Start overlay sequence
        goToNextOverlay();
    }


    // Main function to go to the next overlay
    //  Pre: -1 <= currentPage < overlayDisplays.Length
    //  Post: either goes to the next overlay or destroys object when overlay sequence is finished
    private void goToNextOverlay() {
        Debug.Assert(currentPage >= -1 && currentPage < overlayDisplays.Length);

        // If you are currently on a page, disable gameobject. If you're starting the sequence (currentPage == -1), disable mainPlayerInput and freeze game
        if (currentPage >= 0) {
            overlayDisplays[currentPage].SetActive(false);
        } else {
            mainPlayerInput.enabled = false;
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        // Increment current page
        currentPage++;

        // If current page is within sequence (< overlaysDisplays.Length), just display the newest item in the sequence. If at the end, go back to normal state
        if (currentPage < overlayDisplays.Length) {
            overlayDisplays[currentPage].SetActive(true);
        } else {
            mainPlayerInput.enabled = true;
            Time.timeScale = prevTimeScale;
            Object.Destroy(gameObject);
        }
    }


    // Main event handler for when it's pressed
    public void onContinueSequencePressed(InputAction.CallbackContext value) {
        if (value.started && currentPage < overlayDisplays.Length) {
            goToNextOverlay();
        }
    }
}
