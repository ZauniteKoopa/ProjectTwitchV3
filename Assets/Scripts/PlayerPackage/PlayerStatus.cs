using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class PlayerStatus : ITwitchStatus
{
    // Other reference variables
    [SerializeField]
    private MeshRenderer characterRenderer;
    private Color normalColor;
    [SerializeField]
    private Color deathColor = Color.black;

    // Movement speed variables
    [SerializeField]
    private float baseMovementSpeed = 8.0f;
    private float movementSpeedFactor = 1.0f;
    private float baseAttackSpeedFactor = 1.0f;

    // Health and Invincibility
    [SerializeField]
    private float maxHealth = 60f;
    [SerializeField]
    private float armor = 1.5f;
    private float curHealth;
    private bool isInvincible = false;
    [SerializeField]
    private float invincibilityFrameDuration = 0.6f;
    private readonly object healthLock = new object();
    [SerializeField]
    private Checkpoint checkpoint;

    // Poison vial variables
    private ITwitchInventory inventory;

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
    [SerializeField]
    private float craftingTime = 1.25f;

    private bool canCask = true;
    private bool canCamo = true;
    private bool canContaminate = true;
    private Coroutine runningCaskSequence = null;
    private Coroutine runningContaminateSequence = null;
    private Coroutine runningCamoSequence = null;

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
    [SerializeField]
    private InvisibilityVisionSensor invisSensor;
    private bool inCamofladge = false;
    private int numCamoBuffs = 0;
    private readonly object camoBuffLock = new object();

    [Header("Death Sequence")]
    [SerializeField]
    private float deathShockDuration = 1.5f;
    [SerializeField]
    private float deathFadeDuration = 3.0f;

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

        if (checkpoint == null) {
            Debug.LogWarning("No starting checkpoint set for player. Please make sure that it's set immediately when spawn in or that death's not possible in scene", transform);
        }

        inventory = GetComponent<ITwitchInventory>();
        if (inventory == null) {
            Debug.LogError("No inventory found to get Poison vials from");
        }

        curHealth = maxHealth;
        normalColor = characterRenderer.material.color;
        inventory.addCraftListener(onPlayerCraft);
        initDefaultUI();
    }


    // Main variable to initialize UI
    private void initDefaultUI() {
        mainPlayerUI.displayHealth(curHealth, maxHealth);
        mainPlayerUI.displayCoinsEarned(0);

        mainPlayerUI.displayCamoCooldown(-1.0f, 1.0f);
        mainPlayerUI.displayCaskCooldown(-1.0f, 1.0f);
        mainPlayerUI.displayContaminateCooldown(-1.0f, 1.0f);
        mainPlayerUI.displayCaskAmmoCost(caskCost);

        mainPlayerUI.displayInvisibilityTimer(camoDuration, camoDuration, false);
        invisSensor.makeVisible(false);
    }


    // Main accessor method to get movement speed
    //  Pre: none
    //  Post: returns base movement speed with speed factors applied
    public override float getMovementSpeed() {
        float currentSpeed = baseMovementSpeed * movementSpeedFactor;

        Debug.Assert(currentSpeed >= 0.0f);
        return currentSpeed;
    }


    // Main function to slow down or speed up by a specifed speed factor
    //  Pre: speedFactor > 0.0f. If less than 1, slow. Else, fast
    //  Post: speed is affected accordingly
    public override void affectSpeed(float speedFactor) {
        movementSpeedFactor *= speedFactor;

        // Set movement affects regarding speed
        audioManager.setStepRateFactor(movementSpeedFactor);
    }

    // Main function to get attack rate effect factor
    //  Pre: none
    //  Post: returns a variable > 0.0f;
    public override float getAttackRateFactor() {
        return 1.0f / baseAttackSpeedFactor;
    }


    // Main function to check if the unit is still alive
    //  Pre: none
    //  Post: returns true is unit is still alive
    public override bool isAlive() {
        return curHealth > 0f;
    }


    // Main method to damage player unit
    //  Pre: damage is a number greater than 0
    //  Post: damage is inflicted on player unit and return is damage is successful
    public override bool damage(float dmg, bool isTrue) {
        dmg = (isTrue) ? dmg : IUnitStatus.calculateDamage(dmg, armor);

        lock (healthLock) {
            if (!isInvincible && curHealth > 0f) {
                // Decrement health
                curHealth -= dmg;
                mainPlayerUI.displayHealth(curHealth, maxHealth);

                // If still alive, do I-Frame sequence, else, death
                if (curHealth > 0f) {
                    audioManager.playHurtSound();
                    StartCoroutine(invincibilityFrameSequence());
                } else {
                    audioManager.playDeathSound();
                    StartCoroutine(death());
                }

            }
        }

        return true;
    }


    // Main Private IEnumerator to do invincibility sequence
    private IEnumerator invincibilityFrameSequence() {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityFrameDuration);
        isInvincible = false;
    }

    
    // Main private IEnumerator to do death
    private IEnumerator death() {
        // Trigger death animation 
        characterRenderer.material.color = deathColor;
        yield return new WaitForSeconds(deathShockDuration);

        // Trigger fade out
        mainPlayerUI.executeFadeOut(Color.black, deathFadeDuration);
        yield return new WaitForSeconds(deathFadeDuration);

        reset();
    }

    // Main function to reset unit, especially when player dies
    //  Pre: none
    //  Post: If enemy, reset to passive state, not sensing any enemies
    //        If player, reset all cooldowns to 0 and lose collectibles upon death
    public override void reset() {
        // Rest player status variable and move entire player package to spawnpoint
        checkpoint.respawnPlayer(transform.parent);
        curHealth = maxHealth;
        inventory.clear();

        // Reset cask
        if (runningCaskSequence != null) {
            StopCoroutine(runningCaskSequence);
            runningCaskSequence = null;
            canCask = true;
        }

        // Reset Contaminate
        if (runningContaminateSequence != null) {
            StopCoroutine(runningContaminateSequence);
            runningContaminateSequence = null;
            canContaminate = true;
        }

        // Reset stealth
        if (runningCamoSequence != null) {
            StopCoroutine(runningCamoSequence);
            runningCamoSequence = null;

            canCamo = true;
            inCamofladge = false;
            baseAttackSpeedFactor = 1.0f;
        }

        // Reset UI
        initDefaultUI();
        characterRenderer.material.color = normalColor;
    }


    // Function to access the max health of the player
    //  Pre: none
    //  Post: max health > 0
    public override float getMaxHealth() {
        return maxHealth;
    }


    // Function to set checkpoint
    //  Pre: checkpoint != null
    //  Post: character will now be assigned to this checkpoint
    public override void setCheckpoint(Checkpoint cp) {
        checkpoint = cp;
    }


    // Main method to get access to primary poison vial
    //  Pre: none
    //  Post: returns the primary poison vial that player is using, CAN BE NULL
    public override IVial getPrimaryVial() {
        return inventory.getPrimaryVial();
    }


    // Main event handler function for swapping between 2 vials
    //  Pre: none
    //  Post: swaps primary and secondary vials on the fly
    public override void swapVials() {
        inventory.swapVials();
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

        return inventory.consumePrimaryVial(boltCost);
    }


    // Main function to use cask bullet wth defined cost
    //  Pre: none
    //  Post: returns if successful, If so, reduce primary vial's ammo
    public override bool consumePrimaryVialCask() {
        if (!canCask) {
            mainPlayerUI.displayAbilityCooldownError();
            audioManager.playErrorSound();
            return false;
        }

        bool usedCask = inventory.consumePrimaryVial(caskCost);
        if (usedCask) {
            // If used a cask while camofladge, get out of camofladge
            if (inCamofladge) {
                inCamofladge = false;
            }

            runningCaskSequence = StartCoroutine(caskCooldownSequence());
        } else {
            audioManager.playErrorSound();
            mainPlayerUI.displayVialAmmoError();
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

        runningCaskSequence = null;
        canCask = true;
    }

    // Main function to get permissions to cast contaminate
    //  Pre: bool representing if you are within range of an infected enemy
    //  Post: return if you are allowed. If successful, must wait for cooldown to stop to do it again
    public override bool willContaminate(bool withinContaminateRange) {
        if (!withinContaminateRange) {
            audioManager.playErrorSound();
            mainPlayerUI.displayContaminateRangeError();
            return false;
        }

        if (canContaminate) {
            runningContaminateSequence = StartCoroutine(contaminateCooldownSequence());
            return true;
        }

        audioManager.playErrorSound();
        mainPlayerUI.displayAbilityCooldownError();
        return false;
    }


    // Cask cooldown sequence
    private IEnumerator contaminateCooldownSequence() {
        audioManager.playContaminateSound();
        contaminateUsedEvent.Invoke();

        float timer = contaminateCooldown;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        canContaminate = false;
        mainPlayerUI.displayContaminateCooldown(timer, contaminateCooldown);

        while (timer >= 0f) {
            yield return waitFrame;
            timer -= Time.fixedDeltaTime;

            mainPlayerUI.displayContaminateCooldown(timer, contaminateCooldown);
        }

        contaminateReadyEvent.Invoke();
        runningContaminateSequence = null;
        canContaminate = true;
    }


    // Main function to get permissions to cast camofladge
    //  Pre: none
    //  Post: return true if you can start camofladge. canno camo if in camo sequence
    public override bool willCamofladge() {
        if (canCamo && runningCamoSequence == null) {
            runningCamoSequence = StartCoroutine(camofladgeSequence());
            return true;
        }

        audioManager.playErrorSound();
        mainPlayerUI.displayAbilityCooldownError();
        return false;
    }


    // Camofladge sequence
    //  Pre: none
    //  Post: start camoflage sequence for player
    private IEnumerator camofladgeSequence() {
        canCamo = false;

        // Startup
        audioManager.playStealthCastSound();
        mainPlayerUI.displayCamoCooldown(camoCooldown, camoCooldown);
        characterRenderer.material.color = startupColor;
        yield return new WaitForSeconds(camoStartup);

        yield return goInvisible();

        // Apply attack speed buff
        lock (camoBuffLock) {
            baseAttackSpeedFactor = camoAttackRateBuff;
            characterRenderer.material.color = buffColor;
            numCamoBuffs++;
        }

        Invoke("resetCamofladgeBuff", camoAttackSpeedBuffDuration);

        // Timer to do cooldown IF canCamo is false. If it's true, you killed someone during camo sequence
        if (!canCamo) {
            float timer = camoCooldown;
            WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
            
            while (timer > 0f && !canCamo) {
                yield return waitFrame;
                timer -= Time.fixedDeltaTime;

                mainPlayerUI.displayCamoCooldown(timer, camoCooldown);
            }
            canCamo = true;
        }

        // Set camo running sequence to false
        mainPlayerUI.displayCamoCooldown(0, camoCooldown);
        runningCamoSequence = null;
    }


    // Private helper function for camofladge sequence: main sequence to go invisible
    //  Pre: none
    //  Post: you start invisibility sequence of stealth in which you will go invisible for a short amount of time
    private IEnumerator goInvisible() {
        // Start camofladge
        inCamofladge = true;
        invisSensor.makeVisible(true);
        mainPlayerUI.displayInvisibilityTimer(camoDuration, camoDuration, true);
        characterRenderer.material.color = stealthColor;
        affectSpeed(camoMovementSpeedBuff);

        // Camofladge timer
        float timer = 0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        while (timer < camoDuration && inCamofladge) {
            yield return waitFrame;
            timer += Time.fixedDeltaTime;
            mainPlayerUI.displayInvisibilityTimer(camoDuration - timer, camoDuration, true);
        }

        // camo expires
        inCamofladge = false;
        affectSpeed( 1f / camoMovementSpeedBuff);
        invisSensor.makeVisible(false);
        mainPlayerUI.displayInvisibilityTimer(0, camoDuration, false);
    }


    // Main function to reset camofladge buff
    private void resetCamofladgeBuff() {
        lock (camoBuffLock) {
            numCamoBuffs--;

            // If this was the last camo buff affecting unit, turn back to normal
            if (numCamoBuffs <= 0) {
                characterRenderer.material.color = normalColor;
                baseAttackSpeedFactor = 1.0f;
            }
        }
    }


    // Function to see if you can see the player is visible to enemy
    //  Pre: enemy != null
    //  Post: returns whether the enemy can see the player. Does not consider distance or walls inbetween
    public override bool isVisible(Collider enemy)  {
        return !inCamofladge || invisSensor.isInInvisibilityRange(enemy);
    }


    // Event handler for when stealth has been reset.
    //  Pre: Stealth resets IFF the player has killed at least one unit with expunge
    //  Post: stealth cooldown will reset. HOWEVER, cannot use camofladge when stealth sequence already running
    public override void onStealthReset() {
        canCamo = true;
    }


    // Main function to check if you can do your ultimate
    //  Pre: none
    //  Post: return if ult execution is successful, returns false otherwise
    public override bool willExecuteUltimate() {
        return inventory.willExecutePrimaryUltimate(this);
    }


    // Private event handler function for inventory crafting
    private void onPlayerCraft() {
        StartCoroutine(craftSequence());
    }

    // Sequence for handling crafting
    private IEnumerator craftSequence() {
        stun(true);
        yield return inventory.craftSequence(craftingTime);
        stun(false);
    }


    // Function for when you want to apply health regen status effect
    //  Pre: healthPerFrame >= 0.0f and duration >= 0.0f
    //  Post: applies health regen effect that lasts for duration seconds, healing healthPerFrame every frame
    public override void applyHealthRegenEffect(float healthPerFrame, float duration) {
        StartCoroutine(healthRegenEffectSequence(healthPerFrame, duration));
    }


    // Function for when you want to apply health regen status effect
    //  Pre: healthPerFrame >= 0.0f and duration >= 0.0f
    //  Post: sequence is now running
    private IEnumerator healthRegenEffectSequence(float healthPerFrame, float duration) {
        float timer = 0.0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        // Main loop
        while (timer < duration) {
            yield return waitFrame;

            // Update health
            lock (healthLock) {
                curHealth = Mathf.Min(curHealth + healthPerFrame, maxHealth);
                mainPlayerUI.displayHealth(curHealth, maxHealth);
            }

            // Update timer
            timer += Time.fixedDeltaTime;
        }
    }


    // Function to set movement to true 
    //  Pre: bool representing whether the player is moving or not
    //  Post: enact effects that happen while you're moving or deactivate effects when you aren't
    public override void setMoving(bool isMoving) {
        audioManager.setFootstepsActive(isMoving);
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


    // Main method to get current base attack for sword swings or ranged attacks 
    //  Pre: none
    //  Post: Returns a float that represents base attack (> 0)
    public override float getBaseAttack() {return 0.0f;}
}
