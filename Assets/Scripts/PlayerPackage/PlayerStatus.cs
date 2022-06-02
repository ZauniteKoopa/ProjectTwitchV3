using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerStatus : ITwitchStatus
{
    // Other reference variables
    [SerializeField]
    private MeshRenderer characterRenderer;
    private Color normalColor;

    // Movement speed variables
    [SerializeField]
    private float baseMovementSpeed = 8.0f;
    private float movementSpeedFactor = 1.0f;
    private float baseAttackSpeedFactor = 1.0f;

    // Poison vial variables
    private IVial primaryPoisonVial;
    private IVial secondaryPoisonVial;

    // UI variables
    [SerializeField]
    private ITwitchPlayerUI mainPlayerUI = null;

    [Header("Ability Costs and cooldowns")]
    [SerializeField]
    private int boltCost = 1;
    [SerializeField]
    private int caskCost = 3;
    [SerializeField]
    private float caskCooldown = 10f;
    [SerializeField]
    private float camoCooldown = 15f;
    [SerializeField]
    private float contaminateCooldown = 9f;

    private bool canCask = true;
    private bool canCamo = true;
    private bool canContaminate = true;

    [Header("Camofladge variables")]
    [SerializeField]
    private float camoAttackRateBuff = 1.5f;
    [SerializeField]
    private float camoMovementSpeedBuff = 1.2f;
    [SerializeField]
    private float camoDuration = 12f;
    [SerializeField]
    private float camoAttackSpeedBuffDuration = 5f;
    [SerializeField]
    private float camoStartup = 1f;
    [SerializeField]
    private Color stealthColor;
    [SerializeField]
    private Color buffColor;
    [SerializeField]
    private Color startupColor;
    private bool inCamofladge = false;

    [Header("Audio")]
    [SerializeField]
    private TwitchPlayerAudio audioManager;


    //On awake, initialize poison vials (GET RID OF THIS IN CRAFTING) and UI after UI initialized
    private void Start() {
        // Error check
        if (baseMovementSpeed < 0.0f) {
            Debug.LogError("Player base movement speed cannot be negative: " + transform, transform);
        }

        if (mainPlayerUI == null) {
            Debug.LogError("PlayerStatus not connected to ITwitchPlayerUI object: " + transform, transform);
        }

        if (audioManager == null) {
            Debug.LogError("TwitchPlayerAudio not connected to PlayerStatus to make use of sounds");
        }

        primaryPoisonVial = new PoisonVial(3, 0, 2, 0, 40);
        secondaryPoisonVial = new PoisonVial(0, 2, 0, 3, 40);
        normalColor = characterRenderer.material.color;
        initDefaultUI();
    }


    // Main variable to initialize UI
    private void initDefaultUI() {
        mainPlayerUI.displayHealth(50f, 50f);
        mainPlayerUI.displayCoinsEarned(0);

        mainPlayerUI.displayPrimaryVial(primaryPoisonVial);
        mainPlayerUI.displaySecondaryVial(secondaryPoisonVial);

        mainPlayerUI.displayCamoCooldown(-1.0f, 1.0f);
        mainPlayerUI.displayCaskCooldown(-1.0f, 1.0f);
        mainPlayerUI.displayContaminateCooldown(-1.0f, 1.0f);
        mainPlayerUI.displayCaskAmmoCost(caskCost);
    }


    // Main accessor method to get movement speed
    //  Pre: none
    //  Post: returns base movement speed with speed factors applied
    public override float getMovementSpeed() {
        float currentSpeed = baseMovementSpeed * movementSpeedFactor;

        // Checks if camofladged
        if (inCamofladge) {
            currentSpeed *= camoMovementSpeedBuff;
        }

        Debug.Assert(currentSpeed >= 0.0f);
        return currentSpeed;
    }

    // Main function to get attack rate effect factor
    //  Pre: none
    //  Post: returns a variable > 0.0f;
    public override float getAttackRateFactor() {
        return 1.0f / baseAttackSpeedFactor;
    }


    // Main method to damage player unit
    //  Pre: damage is a number greater than 0
    //  Post: damage is inflicted on player unit
    public override void damage(float dmg) {
        Debug.Log("Player suffered " + dmg + " damage");
        mainPlayerUI.displayHealth(30f, 30f);
    }


    // Main method to get access to primary poison vial
    //  Pre: none
    //  Post: returns the primary poison vial that player is using, CAN BE NULL
    public override IVial getPrimaryVial() {
        return primaryPoisonVial;
    }


    // Main shorthand method to use primary vial. THIS IS THE ONLY ONE CALLING USE VIAL FOR THE PLAYER
    //  Pre: ammoCost >= 0
    //  Post: returns if successful. If so, reduces primary vial's ammo. If ammo is <= 0 afterwards, sets it to null
    private bool usePrimaryVialAmmo(int ammoCost) {
        Debug.Assert(ammoCost >= 0);

        // If vial is an empty vial, return false immediately
        if (primaryPoisonVial == null) {
            return false;
        }

        // If you actually have a vial, use it to test if successful. then see if you run out of poison
        bool useSuccess = primaryPoisonVial.useVial(ammoCost);
        if (primaryPoisonVial.getAmmoLeft() <= 0) {
            primaryPoisonVial = null;
        }

        mainPlayerUI.displayPrimaryVial(primaryPoisonVial);
        return useSuccess;   
    }


    // Main event handler function for swapping between 2 vials
    //  Pre: none
    //  Post: swaps primary and secondary vials on the fly
    public override void swapVials() {
        IVial tempVial = primaryPoisonVial;
        primaryPoisonVial = secondaryPoisonVial;
        secondaryPoisonVial = tempVial;

        mainPlayerUI.displayPrimaryVial(primaryPoisonVial);
        mainPlayerUI.displaySecondaryVial(secondaryPoisonVial);
    }


    // Main function to use bolt bullet wth defined cost
    //  Pre: none
    //  Post: returns if successful, If so, reduce primary vial's ammo
    public override bool consumePrimaryVialBullet() {
        // If using a bullet, get out of camofladge if in it
        if (inCamofladge) {
            inCamofladge = false;
            baseAttackSpeedFactor = camoAttackRateBuff;
        }

        // Play bolt sound, regardless of whether you use poison arrow or weak arrow
        audioManager.playBoltSound();

        return usePrimaryVialAmmo(boltCost);
    }


    // Main function to use cask bullet wth defined cost
    //  Pre: none
    //  Post: returns if successful, If so, reduce primary vial's ammo
    public override bool consumePrimaryVialCask() {
        if (!canCask) {
            return false;
        }

        bool usedCask = usePrimaryVialAmmo(caskCost);
        if (usedCask) {
            // If used a cask while camofladge, get out of camofladge
            if (inCamofladge) {
                inCamofladge = false;
            }

            StartCoroutine(caskCooldownSequence());
        }

        return usedCask;
    }

    
    // Cask cooldown sequence
    private IEnumerator caskCooldownSequence() {
        audioManager.playCaskCastSound();

        float timer = caskCooldown;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        canCask = false;
        mainPlayerUI.displayCaskCooldown(timer, caskCooldown);

        while (timer >= 0f) {
            yield return waitFrame;
            timer -= Time.fixedDeltaTime;

            mainPlayerUI.displayCaskCooldown(timer, caskCooldown);
        }

        canCask = true;
    }

    // Main function to get permissions to cast contaminate
    //  Pre: none
    //  Post: return if you are allowed. If successful, must wait for cooldown to stop to do it again
    public override bool willContaminate() {
        if (canContaminate) {
            StartCoroutine(contaminateCooldownSequence());
            return true;
        }

        return false;
    }


    // Cask cooldown sequence
    private IEnumerator contaminateCooldownSequence() {
        audioManager.playContaminateSound();

        float timer = contaminateCooldown;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        canContaminate = false;
        mainPlayerUI.displayContaminateCooldown(timer, contaminateCooldown);

        while (timer >= 0f) {
            yield return waitFrame;
            timer -= Time.fixedDeltaTime;

            mainPlayerUI.displayContaminateCooldown(timer, contaminateCooldown);
        }

        canContaminate = true;
    }


    // Main function to get permissions to cast camofladge
    //  Pre: none
    //  Post: return true if you can start camofladge. canno camo if in camo sequence
    public override bool willCamofladge() {
        if (canCamo) {
            StartCoroutine(camofladgeSequence());
            return true;
        }

        return false;
    }


    // Camofladge sequence
    private IEnumerator camofladgeSequence() {
        canCamo = false;

        // Startup
        audioManager.playStealthCastSound();
        mainPlayerUI.displayCamoCooldown(camoCooldown, camoCooldown);
        characterRenderer.material.color = startupColor;
        yield return new WaitForSeconds(camoStartup);

        // Start camofladge
        inCamofladge = true;
        characterRenderer.material.color = stealthColor;

        float timer = 0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        while (timer < camoDuration && inCamofladge) {
            yield return waitFrame;
            timer += Time.fixedDeltaTime;
        }

        inCamofladge = false;

        // Apply attack speed buff
        baseAttackSpeedFactor = camoAttackRateBuff;
        characterRenderer.material.color = buffColor;
        Invoke("resetCamofladgeBuff", camoAttackSpeedBuffDuration);

        // Timer to do cooldown
        timer = camoCooldown;

        while (timer > 0f) {
            yield return waitFrame;
            timer -= Time.fixedDeltaTime;

            mainPlayerUI.displayCamoCooldown(timer, camoCooldown);
        }

       canCamo = true;
    }


    // Main function to reset camofladge buff
    private void resetCamofladgeBuff() {
        characterRenderer.material.color = normalColor;
        baseAttackSpeedFactor = 1.0f;
    }



    // --------------------------
    //  Functions to be implemented if want Twitch to share some features with enemies (poison damage, contaminate burst, etc)
    // --------------------------
    

    // Main method to do poison damage (specific to twitch damage)
    //  initDmg: initial, immediate damage applied to enemy, > 0
    //  poison: PoisonVial that will be inflicted to this enemy.
    //  numStacks: number of stacks applied to enemy when doing immediate damage
    //  Post: damage AND poison will be applied to enemy
    public override void poisonDamage(float initDmg, IVial poison, int numStacks) {}


    // Main method to do poison damage (specific to twitch damage)
    //  initDmg: initial, immediate damage applied to enemy, > 0
    //  poison: PoisonVial that will be inflicted to this enemy IFF unit isn't already inflicted with poison
    //  numStacks: number of stacks applied to enemy when doing immediate damage
    //  Post: damage AND poison will be applied to enemy
    public override void weakPoisonDamage(float initDmg, IVial poison, int numStacks) {}


    // Main function to contaminate the unit with the poison they already have
    //  Pre: none
    //  Post: enemy suffers from severe burst damage
    public override void contaminate() {}


    // Main function to check if a unit is poisoned
    //  Pre: none
    //  Post: returns whether or not the unit is poisoned
    public override bool isPoisoned() {return false;}
}
