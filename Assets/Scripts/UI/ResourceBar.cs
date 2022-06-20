using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using TMPro;

public class ResourceBar : MonoBehaviour
{
    // Reference variables: one of these must be valid, bar must be fillable
    [SerializeField]
    private Image resourceBar;
    [SerializeField]
    private TMP_Text resourceText;


    // On awake, error check
    private void Awake() {
        if (resourceBar == null && resourceText == null) {
            Debug.LogError("Resource bar of " + transform + "not connected to any UI elements!", transform);
        }
    }


    // Main public function to set resource bar settings
    //  Pre: maxResources >= curResources && maxResources > 0f
    //  Post: set resource bar and text if not null
    public void setStatus(float curResources, float maxResources) {
        Debug.Assert(maxResources >= curResources && maxResources > 0f);

        // Check for negative cur value
        curResources = Mathf.Max(0f, curResources);

        // Set resource bar
        if (resourceBar != null) {
            resourceBar.fillAmount = curResources / maxResources;
        }

        // Set resource text, rounding curResource to the nearest 100th
        if (resourceText != null) {
            curResources = (Mathf.Round(curResources * 100f) / 100f);
            resourceText.text = curResources + "/" + maxResources;
        }
    }


    // Main function to change the color of this resource bar
    //  Pre: color is not null
    public void setColor(Color color) {
        if (resourceBar != null) {
            resourceBar.color = color;
        }
    }
}