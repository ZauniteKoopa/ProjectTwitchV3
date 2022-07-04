using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TemporaryErrorMessage : MonoBehaviour
{
    // Reference variables
    private TMP_Text errorText;
    [SerializeField]
    private float errorTime = 1.0f;
    private Coroutine runningErrorSequence = null;

    
    // Awake function to set up variables
    private void Awake() {
        errorText = GetComponent<TMP_Text>();

        if (errorText == null) {
            Debug.LogError("No TMP_Text found with this component", transform);
        }

        if (errorTime <= 0.0f) {
            Debug.LogError("Error time not configured properly for temporary error message", transform);
        }

        errorText.gameObject.SetActive(false);
    }


    // Main public function to execute error message
    //  Pre: error message is the message you want to display
    //  Post: Runs error message sequence with that specified error message in mind
    public void executeErrorMessage(string errorMessage) {
        // Make sure only 1 error sequence coroutine is running at any given point
        if (runningErrorSequence != null) {
            StopCoroutine(runningErrorSequence);
        }

        // Start the coroutine
        StartCoroutine(errorMessage);
    }


    // Private IEnumerator sequence for doing error message
    //  Pre: error message to display
    //  Post: shows error message briefly before disappearing abruptly
    private void errorMessageSequence(string errorMessage) {
        // Display error message
        errorText.text = errorMessage;
        errorText.gameObject.SetActive(true);

        // Wait for the specified duration
        yield return new WaitForSeconds(errorTime);

        // make error message disappear
        errorText.gameObject.SetActive(false);
    }
}
