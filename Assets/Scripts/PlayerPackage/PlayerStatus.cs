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
    private float attackMultiplier = 1.0f;

    // Side effects
    private bool manic = false;
    private PriorityQueue<TimedStatusEffect> activeHealingRegens = new PriorityQueue<TimedStatusEffect>();
    private readonly object statusEffectQueueLock = new object();

    // Health and Invincibility
    [SerializeField]
    private float maxHealth = 60f;
    [SerializeField]
    private float armor = 1.5f;
    private float armorMultiplier = 1.0f;

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
        inventory.vialExecutionEvent.AddListener(onStealthReset);
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

        invisSensor.makeVisible(false);

        if (statusDisplay != null) {
            statusDisplay.clear();
        }

        if (statusEffectVFXs != null) {
            statusEffectVFXs.clear();
        }
    }


    // Main accessor method to get movement speed
    //  Pre: none
    //  Post: returns base movement speed with speed factors applied
    public override float getMovementSpeed() {
        float currentSpeed = baseMovementSpeed * movementSpeedFactor;

        Debug.Assert(currentSpeed >= 0.0f);
        return currentSpeed;
    }


    // Main function to make this unit manic if they aren't manic already or get rid of manic if they are
    //  MANIC: attack increases by 1.5 its original value BUT armor decreases by 0.5 that value
    //  Pre: 0.0 < manicIntensity < 1.0f;
    public override void makeManic(bool willManic, float manicIntensity) {
        Debug.Assert(manicIntensity > 0.0f && manicIntensity < 1.0f);
        
        // Update UI display
        if (statusDisplay != null) {
            statusDisplay.displayManic(willManic);
        }

        // Update vfx display
        if (statusEffectVFXs != null) {
            statusEffectVFXs.displayManic(willManic);
        }

        // do functional effects
        if (manic != willManic) {
            manic = willManic;

            armorMultiplier = (manic) ? 1.0f - manicIntensity : 1.0f; 
            attackMultiplier = (manic) ? 1.0f + manicIntensity : 1.0f;
        }
    }

    
    // Main function to get attackChangeFactor
    //  Post: returns the attack multiplier for this unit
    public override float getAttackMultiplier() {
        return attackMultiplier;
    }


    // Main function to slow down or speed up by a specifed speed factor
    //  Pre: speedFactor > 0.0f. If less than 1, slow. Else, fast
    //  Post: speed is affected accordingly
    public override void affectSpeed(float speedFactor) {
        movementSpeedFactor *= speedFactor;

        // Set movement affects regarding speed
        audioManager.setStepRateFactor(movementSpeedFactor);

        if (statusDisplay != null) {
            statusDisplay.displaySpeedStatus(movementSpeedFactor);
        }
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


    // Main method to inflict basic damage on unit
    //  Pre: damage is a number greater than 0, isTrue indicates if its true damage. true damage is not affected by armor and canCrit: can the damage given crit
    //  Post: unit gets inflicted with damage 
    public override bool damage(float dmg, bool isTrue, bool canCrit = false) {
        dmg = (isTrue) ? dmg : IUnitStatus.calculateDamage(dmg, armor * armorMultiplier);

        // Apply damagePopup if it's possible. Round it to tenths so that you don't get ugly decimals
        if (damagePopupPrefab != null && dmg > 0.0f && !isInvincible) {
            TextPopup dmgPopup = Object.Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
            dmgPopup.SetUpPopup("" + (Mathf.Round(dmg * 10f) / 10f), transform);
        }

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

        resetEvent.Invoke();
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
            contaminateReadyEvent.Invoke();
            canContaminate = true;
        }

        // Reset stealth
        if (runningCamoSequence != null) {
            StopCoroutine(runningCamoSequence);
            runningCamoSequence = null;

            canCamo = true;
            inCamofladge = false;
            baseAttackSpeedFactor = 1.0f;
            movementSpeedFactor = 1.0f;
        }

        // reset manic
        manic = false;
        armorMultiplier = 1.0f;
        attackMultiplier = 1.0f;

        // reset healing by stopping all healing coroutines
        lock (statusEffectQueueLock) {
            while (!activeHealingRegens.IsEmpty()) {
                TimedStatusEffect curEffect = activeHealingRegens.Dequeue();
                StopCoroutine(curEffect.statusEffectSequence);
            }
        }

        clearStun();

        // Reset UI
        initDefaultUI();

        // cleanup
        PlayerCameraController.instantReset();
        StartCoroutine(cleanup());
        characterRenderer.material.color = normalColor;
    }


    // Function to cleanup everything in the world
    private IEnumerator cleanup() {
        // Find all objects to clean up
        Loot[] livingLoot = Object.FindObjectsOfType<Loot>();
        IBattleUltimate[] runningBattleUltimates = Object.FindObjectsOfType<IBattleUltimate>();
        AbstractStraightProjectile[] activeProjectiles = Object.FindObjectsOfType<AbstractStraightProjectile>();
        BasicEnemySlowZone[] activeSlowZones = Object.FindObjectsOfType<BasicEnemySlowZone>();

        // For each loot, push them to shadow realm for trigger box handling
        foreach (Loot loot in livingLoot) {
            loot.transform.position = 1000000000f * Vector3.up;
        }

        // Reset each lingering ult
        foreach (IBattleUltimate ult in runningBattleUltimates) {
            ult.reset();
        }

        // For each active projectile, dsestroy the projectile
        foreach (AbstractStraightProjectile proj in activeProjectiles) {
            Object.Destroy(proj.gameObject);
        }

        // Destroy each slow zone
        foreach (BasicEnemySlowZone slowZone in activeSlowZones) {
            slowZone.destroyZone();
        }

        yield return new WaitForSeconds(0.1f);

        // Destroy the object once you're sure that they won't interfere with important trigger box
        foreach (Loot loot in livingLoot) {
            Object.Destroy(loot);
        }
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

        // Do surprise effects
        inventory.utilizePlayerAura(AuraType.SURPRISE);

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
        characterRenderer.material.color = stealthColor;
        affectSpeed(camoMovementSpeedBuff);

        if (statusDisplay != null) {
            statusDisplay.displayStealth(true, camoDuration, camoDuration);
        }

        if (statusEffectVFXs != null) {
            statusEffectVFXs.displayStealth(true);
        }

        // Camofladge timer
        float timer = 0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        while (timer < camoDuration && inCamofladge) {
            yield return waitFrame;
            timer += Time.fixedDeltaTime;

            if (statusDisplay != null) {
                statusDisplay.displayStealth(true, camoDuration - timer, camoDuration);
            }
        }

        // camo expires
        inCamofladge = false;
        affectSpeed( 1f / camoMovementSpeedBuff);
        invisSensor.makeVisible(false);

        if (statusDisplay != null) {
            statusDisplay.displayStealth(false, 0f, camoDuration);
        }

        if (statusEffectVFXs != null) {
            statusEffectVFXs.displayStealth(false);
        }
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
    public override bool willExecuteUltimate(Vector3 dest) {
        bool usedUltimate = inventory.willExecutePrimaryUltimate(this, dest);

        // Pop out of stealth
        if (usedUltimate && inCamofladge) {
            inCamofladge = false;
        }

        return usedUltimate;
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
    //  Pre: 0f <= healthPercentHealed <= 1.0f and duration >= 0.0f
    //  Post: applies health regen effect that lasts for duration seconds, healing healthPercent of max health over that duration
    public override void applyHealthRegenEffect(float healthPercentHealed, float duration) {
        Debug.Assert(healthPercentHealed >= 0.0f && healthPercentHealed <= 1.0f);
        Debug.Assert(duration >= 0.0f);

        float healingAmount = maxHealth * healthPercentHealed;
        float numTicks = duration / Time.fixedDeltaTime;
        float healPerFrame = healingAmount / numTicks;

        Coroutine curHealthRegen = StartCoroutine(healthRegenEffectSequence(healPerFrame, duration));
        TimedStatusEffect curEffect = new TimedStatusEffect(duration, curHealthRegen);
        lock (statusEffectQueueLock) {
            activeHealingRegens.Enqueue(curEffect);
        }
    }


    // Function for when you want to apply health regen status effect
    //  Pre: healthPerFrame >= 0.0f and duration >= 0.0f
    //  Post: sequence is now running
    private IEnumerator healthRegenEffectSequence(float healthPerFrame, float duration) {
        float timer = 0.0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        if (statusDisplay != null) {
            statusDisplay.displayHealing(true);
        }

        if (statusEffectVFXs != null) {
            statusEffectVFXs.displayHealing(true);
        }

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

        if (statusDisplay != null) {
            statusDisplay.displayHealing(false);
        }

        if (statusEffectVFXs != null) {
            statusEffectVFXs.displayHealing(false);
        }

        // pop out from healthRegenEffect sequence
        lock (statusEffectQueueLock) {
            activeHealingRegens.Dequeue();
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
    //  canCrit: can the damage given crit
    //  Post: damage AND poison will be applied to enemy
    public override void poisonDamage(float initDmg, IVial poison, int numStacks, bool canCrit = false) {}


    // Main method to do poison damage (specific to twitch damage)
    //  initDmg: initial, immediate damage applied to enemy, > 0
    //  poison: PoisonVial that will be inflicted to this enemy IFF unit isn't already inflicted with poison
    //  numStacks: number of stacks applied to enemy when doing immediate damage
    //  canCrit: can the damage given crit
    //  Post: damage AND poison will be applied to enemy IFF enemy had no poison initially
    public override void weakPoisonDamage(float initDmg, IVial poison, int numStacks, bool canCrit = false) {}


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
