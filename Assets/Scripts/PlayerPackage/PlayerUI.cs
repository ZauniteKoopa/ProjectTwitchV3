using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class PlayerUI : ITwitchPlayerUI
{
    [Header("Health")]
    [SerializeField]
    private ResourceBar[] healthBars;

    [Header("Coin UI")]
    [SerializeField]
    private INumberDisplay coinDisplay;

    [Header("Primary Vial UI")]
    [SerializeField]
    private ResourceBar[] primaryAmmoBars;
    [SerializeField]
    private INumberDisplay potencyStatDisplay;
    [SerializeField]
    private INumberDisplay poisonStatDisplay;
    [SerializeField]
    private INumberDisplay reactivityStatDisplay;
    [SerializeField]
    private INumberDisplay stickinessStatDisplay;

    [Header("Secondary Vial UI")]
    [SerializeField]
    private INumberDisplay secondaryAmmoCount;

    [Header("Ability Icons")]
    [SerializeField]
    private ResourceBar camofladgeIcon;
    private Image camofladgeImage;
    [SerializeField]
    private ResourceBar caskIcon;
    [SerializeField]
    private INumberDisplay caskAmmoCostDisplay;
    private Image caskImage;
    [SerializeField]
    private ResourceBar contaminateIcon;
    private Image contaminateImage;
    [SerializeField]
    private Color readyColor;
    [SerializeField]
    private Color disabledColor;


    // Awake function to error check
    private void Awake() {
        // Error check health
        if (healthBars.Length == 0) {
            Debug.LogError("No health bar connected to player UI: " + transform, transform);
        }

        // Error check coin UI
        if (coinDisplay == null) {
            Debug.LogError("No number display to show money for player UI: " + transform, transform);
        }

        errorCheckVialUI();
        errorCheckAbilityUI();

        // Set images
        caskImage = caskIcon.GetComponent<Image>();
        contaminateImage = contaminateIcon.GetComponent<Image>();
        camofladgeImage = camofladgeIcon.GetComponent<Image>();
    }

    
    // Private helper function to error check vial UI
    private void errorCheckVialUI() {
        if (primaryAmmoBars.Length == 0) {
            Debug.LogError("No ammo bars for primary vial UI for player UI: " + transform, transform);
        }

        if (potencyStatDisplay == null || poisonStatDisplay == null || reactivityStatDisplay == null && stickinessStatDisplay == null) {
            Debug.LogError("Stat displays for primary vial not correctly set up for player UI: " + transform, transform);
        }

        if (secondaryAmmoCount == null) {
            Debug.LogError("Ammo count for secondary vial not connected to player UI: " + transform, transform);
        }
    }


    // Private helper method to error check ability UI
    private void errorCheckAbilityUI() {
        if (contaminateIcon == null || contaminateIcon.GetComponent<Image>() == null) {
            Debug.Log("Contaminate icon not set up correctly or not connected, make sure an image is attached to icon: " + transform, transform);
        }

        if (caskIcon == null || caskIcon.GetComponent<Image>() == null && caskAmmoCostDisplay == null) {
            Debug.Log("Cask icon not set up correctly or not connected, make sure an image is attached to icon: " + transform, transform);
        }

        if (camofladgeIcon == null || camofladgeIcon.GetComponent<Image>() == null) {
            Debug.Log("Camofladge icon not set up correctly or not connected, make sure an image is attached to icon: " + transform, transform);
        }
    }

    // Main function to update health bars of player
    //  Pre: curHealth <= maxHealth && maxHealth > 0
    //  Post: updates all health bars and other related to UI to match numbers
    public override void displayHealth(float curHealth, float maxHealth) {
        Debug.Assert(curHealth <= maxHealth && maxHealth > 0f);

        foreach (ResourceBar healthBar in healthBars) {
            healthBar.setStatus(curHealth, maxHealth);
        }
    }


    // Main function to display information about primary vial
    //  Pre: primaryVial CAN either be non-null or null
    //  Post: updates primary vial related UI to reflect vial if not null or an empty vial
    public override void displayPrimaryVial(IVial primaryVial) {
        // Get stat information
        float displayedAmmo = (primaryVial == null) ? 0 : primaryVial.getAmmoLeft();
        float maxAmmo = (primaryVial == null) ? 60 : primaryVial.getMaxVialSize();

        // Update Ammo bars
        foreach (ResourceBar ammoBar in primaryAmmoBars) {
            ammoBar.setStatus(displayedAmmo, maxAmmo);
        }

        // Update stat information
        if (primaryVial == null) {
            potencyStatDisplay.displayNumber(0);
            poisonStatDisplay.displayNumber(0);
            reactivityStatDisplay.displayNumber(0);
            stickinessStatDisplay.displayNumber(0);
        } else {
            Dictionary<string, int> vialStats = primaryVial.getStats();

            potencyStatDisplay.displayNumber(vialStats["Potency"]);
            poisonStatDisplay.displayNumber(vialStats["Poison"]);
            reactivityStatDisplay.displayNumber(vialStats["Reactivity"]);
            stickinessStatDisplay.displayNumber(vialStats["Stickiness"]);
        }

    }

    
    // Main function to display information about secondary vial
    //  Pre: secondary vial can either be non-null OR null
    //  Post: updates secondary vial related UI to reflect vial if not null or an empty vial
    public override void displaySecondaryVial(IVial secondaryVial) {
        int displayedAmmo = (secondaryVial == null) ? 0 : secondaryVial.getAmmoLeft();
        secondaryAmmoCount.displayNumber(displayedAmmo);
    }


    // Main function to display camofladge cooldown status
    //  Pre: curCooldown <= maxCooldown && maxCooldown > 0
    //  Post: updates all camo cooldown related UI
    public override void displayCamoCooldown(float curCooldown, float maxCooldown) {
        Debug.Assert(curCooldown <= maxCooldown & maxCooldown > 0.0f);

        camofladgeIcon.setStatus(curCooldown, maxCooldown);
        camofladgeImage.color = (curCooldown > 0.0f) ? disabledColor : readyColor;
    }


    // Main function to display cask cooldown information
    //  Pre: curCooldown <= maxCooldown && maxCooldown > 0
    //  Post: updates all cask cooldown related UI
    public override void displayCaskCooldown(float curCooldown, float maxCooldown) {
        Debug.Assert(curCooldown <= maxCooldown & maxCooldown > 0.0f);

        caskIcon.setStatus(curCooldown, maxCooldown);
        caskImage.color = (curCooldown > 0.0f) ? disabledColor : readyColor;
    }


    // Main function to display contaminate cooldown information
    //  Pre: curCooldown <= maxCooldown && maxCooldown > 0
    //  Post: updates all contaminate cooldown UI
    public override void displayContaminateCooldown(float curCooldown, float maxCooldown) {
        Debug.Assert(curCooldown <= maxCooldown & maxCooldown > 0.0f);

        contaminateIcon.setStatus(curCooldown, maxCooldown);
        contaminateImage.color = (curCooldown > 0.0f) ? disabledColor : readyColor;
    }


    // Main function to display coins information
    //  Pre: numCoins >= 0
    //  Post: updates all coin related UI to match information
    public override void displayCoinsEarned(int numCoins) {
        Debug.Assert(numCoins >= 0);

        coinDisplay.displayNumber(numCoins);
    }


    // Main function to display cask ammo cost
    //  Pre: ammoCost >= 0
    //  Post: cask ammo cost information is updated on UI
    public override void displayCaskAmmoCost(int ammoCost) {
        Debug.Assert(ammoCost >= 0);

        caskAmmoCostDisplay.displayNumber(ammoCost);
    }
}
