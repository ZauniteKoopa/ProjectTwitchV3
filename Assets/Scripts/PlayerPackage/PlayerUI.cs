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
    private VialIcon[] primaryVialDisplays;

    [Header("Secondary Vial UI")]
    [SerializeField]
    private VialIcon[] secondaryVialDisplays;

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
    private Color readyColor = Color.white;
    [SerializeField]
    private Color disabledColor;

    [Header("FadeOut Screen")]
    [SerializeField]
    private Image fadeOutScreen;
    [SerializeField]
    private float solidFadeDuration = 0.5f;
    private Coroutine currentFadeSequence;

    [Header("Camo Sequence")]
    [SerializeField]
    private ResourceBar camoTimerBar;


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

        if (fadeOutScreen == null) {
            Debug.LogWarning("No fade out screen detected, lose ability to do screen fade for playerUI: " + transform, transform);
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
        if (primaryVialDisplays.Length == 0 || secondaryVialDisplays.Length == 0) {
            Debug.LogError("A vial display is missing for default UI", transform);
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
        if (caskImage == null) {
            caskImage = caskIcon.GetComponent<Image>();
        }

        foreach (VialIcon primaryVialDisplay in primaryVialDisplays) {
            primaryVialDisplay.DisplayVial(primaryVial);
        }
        
        caskImage.color = primaryVial != null ? primaryVial.getColor() : disabledColor;
    }

    
    // Main function to display information about secondary vial
    //  Pre: secondary vial can either be non-null OR null
    //  Post: updates secondary vial related UI to reflect vial if not null or an empty vial
    public override void displaySecondaryVial(IVial secondaryVial) {
        foreach (VialIcon secondaryVialDisplay in secondaryVialDisplays) {
            secondaryVialDisplay.DisplayVial(secondaryVial);
        }
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
        //caskImage.color = (curCooldown > 0.0f) ? disabledColor : readyColor;
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


    // Main function to execute fade out sequence
    //  Pre: fadeColor is the solid color that you want to fade to, and duration is the time it takes to fade to that color
    //  Post: screen will fade to fadeColor in duration seconds
    public override void executeFadeOut(Color fadeColor, float duration) {
        if (currentFadeSequence != null) {
            StopCoroutine(currentFadeSequence);
            currentFadeSequence = null;
        }

        currentFadeSequence = StartCoroutine(fadeOutSequence(fadeColor, duration));
    }


    // Private IEnumerator to do fadeOut
    private IEnumerator fadeOutSequence(Color fadeColor, float duration) {
        fadeOutScreen.gameObject.SetActive(true);
        fadeOutScreen.color = Color.clear;

        float timer = 0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        float screenFadeOutTime = duration - solidFadeDuration;

        while (timer < screenFadeOutTime) {
            yield return waitFrame;

            timer += Time.fixedDeltaTime;
            fadeOutScreen.color = Color.Lerp(Color.clear, fadeColor, timer / screenFadeOutTime);
        }

        fadeOutScreen.color = fadeColor;
        yield return new WaitForSeconds(solidFadeDuration);
        fadeOutScreen.color = Color.clear;
        fadeOutScreen.gameObject.SetActive(false);

        currentFadeSequence = null;
    }


    // Main function to display invisibility timer
    //  Pre: timeLeft <= maxTime  && 0 < maxTime && isVisible just references whether or not this timer should be visible
    //  Post: If isVisible is true, updates timer with current progress. Else, just disable timer
    public override void displayInvisibilityTimer(float timeLeft, float maxTime, bool isVisible) {
        Debug.Assert(timeLeft <= maxTime && maxTime > 0);

        camoTimerBar.gameObject.SetActive(isVisible);

        if (isVisible) {
            camoTimerBar.setStatus(timeLeft, maxTime);
        }
    }
}
