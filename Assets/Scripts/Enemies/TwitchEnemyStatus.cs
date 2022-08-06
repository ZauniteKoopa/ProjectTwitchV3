using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// General class for an enemy within the project Twitch game
public class TwitchEnemyStatus : ITwitchUnitStatus
{
    // Instance variables concerning health
    [Header("Base stats")]
    [SerializeField]
    protected float maxHealth = 10;
    protected float curHealth;
    protected readonly object healthLock = new object();

    // Movement speed variables
    [SerializeField]
    private float baseMovementSpeed = 4.0f;
    private float movementSpeedFactor = 1.0f;

    // Attack stat
    [SerializeField]
    private float baseAttack = 5.0f;
    [SerializeField]
    private float baseArmor = 1f;
    private float armorMultiplier = 1.0f;
    private float attackMultiplier = 1.0f;

    // Poison management damage
    private IVial currentPoison = null;
    private int numPoisonStacks = 0;
    private readonly object poisonLock = new object();

    // Poison timer
    private const int MAX_STACKS = 6;
    private const float MAX_POISON_TICK_TIME = 6.0f;
    private float poisonTimer = 0.0f;
    private Coroutine poisonDotRoutine = null;

    // Enemy AURA
    [Header("Enemy Aura")]
    [SerializeField]
    private EnemyAura enemyAura;

    // Loot drops
    [Header("Loot variables")]
    [SerializeField]
    private int maxLootDrops = 2;
    [SerializeField]
    private int minLootDrops = 2;
    [SerializeField]
    private Loot[] possibleLoot;
    [SerializeField]
    private float minLootDistance = 0.25f;
    [SerializeField]
    private float maxLootDistance = 2.5f;

    // UI variables
    [Header("UI Variables")]
    [SerializeField]
    private ResourceBar healthBar = null;
    [SerializeField]
    private INumberDisplay poisonStackDisplay = null;

    // Events
    [Header("Death / Reset")]
    private Vector3 spawnPoint; 
    public UnityEvent enemyResetEvent;
    [SerializeField]
    private SpawnInEffect spawnInEffect;
    [SerializeField]
    private float spawnInTime = 1.25f;

    
    // Status effects
    private bool isVolatile = false;
    private Coroutine volatileSequence = null;

    private bool manic = false;


    // Audio
    private EnemyAudioManager enemyAudio;


    // On awake, set up variables and error check
    private void Awake() {
        // Error check
        if (maxHealth <= 0.0f) {
            Debug.LogError("Unit's max health is less than or equal to 0: " + transform, transform);
        }

        if (maxLootDrops < minLootDrops || maxLootDrops < 0) {
            Debug.LogError("Loot drop max and min counts are invalid for this unit: " + transform, transform);
        }

        if (healthBar == null) {
            Debug.LogWarning("No health bar connected to this unit. HP will not be visible to player: " + transform, transform);
        }

        if (poisonStackDisplay == null) {
            Debug.LogWarning("No poison stack display connected to this unit. Stacks will not be visible to player: " + transform, transform);
        }

        if (enemyAura == null) {
            Debug.LogWarning("No Enemy aura detected for this enemy, Aura based side effects of vial will not work", transform);
        }

        if (spawnInEffect == null) {
            Debug.LogWarning("No spawn in effect detected for current enemy. When spawning in within a room, the enemy will just appear instantly with no anticipation", transform);
        } else {
            spawnInEffect.effectFinishedEvent.AddListener(onSpawnInEffectFinished);
        }

        enemyAudio = GetComponent<EnemyAudioManager>();
        if (enemyAudio == null) {
            Debug.LogError("No audio manager connected to enemy status: " + transform, transform);
        }

        // Affect footsteps
        float currentModifier = movementSpeedFactor;
        currentModifier *= (currentPoison == null) ? 1f : currentPoison.getStackSlowness(numPoisonStacks);
        enemyAudio.setStepRateFactor(currentModifier);

        // Set variables
        spawnPoint = transform.position;
        curHealth = maxHealth;
        if (healthBar != null) {
            healthBar.setStatus(curHealth, maxHealth);
        }

        // Initialize other variables in child classes
        statusEffectVFXs.clear();
        initialize();
    }


    // Main function to do additional initialization for child classes
    protected virtual void initialize() {}


    // Main method to get current movement speed considering all speed status effects on unit
    //  Pre: none
    //  Post: Returns movement speed with speed status effects in mind
    public override float getMovementSpeed() {
        float trueMovementSpeed = baseMovementSpeed * movementSpeedFactor;

        // If poisoned, get stack slowness
        if (currentPoison != null) {
            trueMovementSpeed *= currentPoison.getStackSlowness(numPoisonStacks);
        }

        return (canMove()) ? trueMovementSpeed : 0f;
    }


    // Main function to slow down or speed up by a specifed speed factor
    //  Pre: speedFactor > 0.0f. If less than 1, slow. Else, fast
    //  Post: speed is affected accordingly
    public override void affectSpeed(float speedFactor) {
        movementSpeedFactor *= speedFactor;

        // Affect footsteps
        float currentModifier = movementSpeedFactor;
        currentModifier *= (currentPoison == null) ? 1f : currentPoison.getStackSlowness(numPoisonStacks);

        // display UI
        if (statusDisplay != null) {
            statusDisplay.displaySpeedStatus(currentModifier);
        }

        if (enemyAudio != null) {
            enemyAudio.setStepRateFactor(currentModifier);
        }
    }


    // Main method to get current base attack for sword swings or ranged attacks 
    //  Pre: none
    //  Post: Returns a float that represents base attack (> 0)
    public override float getBaseAttack() {
        return baseAttack * attackMultiplier;
    }


    // Main function to handle poison loop
    //  Pre: poisonTimer < MAX_POISON_TICK_TIME
    //  Post: does damage over time using IEnumerators
    private IEnumerator poisonDotLoop() {
        Debug.Assert(poisonTimer <= MAX_POISON_TICK_TIME);
        
        // Wait for first poison tick
        float currentTickDuration = 1f;
        lock (poisonLock) {
            currentTickDuration = currentPoison.getPoisonDecayRate();
        }

        yield return new WaitForSeconds(currentTickDuration);

        // Main tick loop
        bool onLastTick = false;
        float auraTimer = 0f;
        while (currentPoison != null && !onLastTick) {

            // Inflict poison damage and update tick status
            float poisonTickDamage;
            int currentStacks;
            IVial tempVial;

            lock(poisonLock) {
                tempVial = currentPoison;
                poisonTickDamage = currentPoison.getPoisonDamage(numPoisonStacks);

                currentTickDuration = currentPoison.getPoisonDecayRate();
                currentStacks = numPoisonStacks;

                poisonTimer += currentTickDuration;
                auraTimer = currentPoison.isEnemyAuraPresent(numPoisonStacks) ? auraTimer + currentTickDuration : 0f;

                onLastTick = poisonTimer >= MAX_POISON_TICK_TIME;
            }

            checkAutoExecution(tempVial, currentStacks);
            damage(poisonTickDamage, true);

            // Apply aura effects if poison side effect matches "CONTAGION"
            if (enemyAura != null && currentPoison.applyEnemyAuraEffectsTimed(enemyAura, AuraType.ENEMY_TIMED, currentStacks, auraTimer)) {
                auraTimer = 0f;
            }

            // Wait for the next tick
            if (!onLastTick) {
                yield return new WaitForSeconds(currentTickDuration);
            }
        }

        // At the end of the last tick, get rid of poison
        lock (poisonLock) {
            // Only invoke event if currentPoison not null before
            if (currentPoison != null) {
                unitCurePoisonEvent.Invoke();
            }

            poisonTimer = 0f;
            numPoisonStacks = 0;
            currentPoison = null;
            poisonDotRoutine = null;

            if (poisonStackDisplay != null) {
                poisonStackDisplay.displayNumber(numPoisonStacks);
            }

            if (enemyAura != null) {
                enemyAura.setActive(false);
            }

            // display UI
            if (statusDisplay != null) {
                statusDisplay.displaySpeedStatus(movementSpeedFactor);
            }
        }
    }


    // Main function to check if the unit is still alive
    //  Pre: none
    //  Post: returns true is unit is still alive
    public override bool isAlive() {
        return curHealth > 0f;
    }


    // Main protected function to check execution
    //  Pre: pv can be null and pStacks >= 0
    //  Post: if you meet infected vial's conditions, kill this unit immediately
    protected virtual void checkAutoExecution(IVial pv, int pStacks) {
        Debug.Assert(pStacks >= 0 && pStacks <= 6);

        if (pv != null) {
            if (pv.canAutoExecute(false, curHealth / maxHealth, pStacks)) {
                damage(curHealth, true);
            }
        }
    }


    // Main handle volatile side effect
    //  Pre: deltaTime > 0f
    //  Post: handle side effect and do damage
    private IEnumerator volatileSideEffect(float volatileDuration) {
        // Turn isVolatile to true (you have cured poison because poison turned to volatile, helps contamination handler)
        isVolatile = true;
        unitCurePoisonEvent.Invoke();
        if (statusDisplay != null) {
            statusDisplay.displayVolatile(true);
        }

        if (statusEffectVFXs != null) {
            statusEffectVFXs.displayImpendingDoomHalo(volatileDuration, transform);
        }

        // Main timer loop
        float timer = 0.0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        while (timer < volatileDuration) {
            yield return waitFrame;
            timer += Time.fixedDeltaTime;
        }

        // Do contaminate damage
        int tempStacks;
        IVial tempVial;
        lock (poisonLock) {
            tempVial = currentPoison;
            tempStacks = numPoisonStacks;
        }

        if (tempVial != null) {
            damage(tempVial.getContaminateDamage(tempStacks), false);

            // Apply aura damage if possible
            if (enemyAura != null) {
                tempVial.applyEnemyAuraEffects(enemyAura, AuraType.RADIOACTIVE_EXPUNGE, tempStacks);
            }
        }

        // Reset stealth if low (canAutoExecute also resets stealth)
        tempVial.canAutoExecute(false, curHealth / maxHealth, tempStacks);

        // Clear out status effects and poison
        clearPoison();
        isVolatile = false;
        volatileSequence = null;
        if (statusDisplay != null) {
            statusDisplay.displayVolatile(false);
        }

    }


    // Main function to make this unit manic if they aren't manic already or get rid of manic if they are
    //  MANIC: attack increases by 1.5 its original value BUT armor decreases by 0.5 that value
    public override void makeManic(bool willManic, float manicIntensity) {
        Debug.Assert(manicIntensity > 0.0f && manicIntensity < 1.0f);

        // Update UI
        if (statusDisplay != null) {
            statusDisplay.displayManic(willManic);
        }

        // Update vfx display
        if (statusEffectVFXs != null) {
            statusEffectVFXs.displayManic(willManic);
        }

        // Update stats
        if (manic != willManic) {
            manic = willManic;

            armorMultiplier = (manic) ? 1.0f - manicIntensity : 1.0f; 
            attackMultiplier = (manic) ? 1.0f + manicIntensity : 1.0f;
        }
    }

    
    // Main function to get attackChangeFactor
    public override float getAttackMultiplier() {
        return attackMultiplier;
    }


    // Main method to inflict basic damage on unit
    //  Pre: damage is a number greater than 0
    //  Post: unit gets inflicted with damage and returns if damage was successful
    public override bool damage(float dmg, bool isTrue) {
        // Calculate damage
        dmg = (isTrue) ? dmg : IUnitStatus.calculateDamage(dmg, baseArmor * armorMultiplier);

        // Apply damagePopup if it's possible. Round it to tenths so that you don't get ugly decimals
        if (damagePopupPrefab != null && dmg > 0.0f) {
            TextPopup dmgPopup = Object.Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
            dmgPopup.SetUpPopup("" + (Mathf.Round(dmg * 10f) / 10f), transform);
        }

        // Apply damage. Use a lock to make sure changes to health are synchronized
        lock(healthLock) {

            // Only change health is still alive. No need to kill unit more than once
            if (curHealth > 0.0f){
                curHealth -= dmg;

                if (healthBar != null) {
                    healthBar.setStatus(curHealth, maxHealth);
                }

                // Check death condition
                if (curHealth <= 0.0f && gameObject.activeSelf) {
                    StartCoroutine(death());
                }
            }
        }

        return true;
    }


    // Main method to do poison damage (specific to twitch damage)
    //  initDmg: initial, immediate damage applied to enemy, >= 0
    //  poison: PoisonVial that will be inflicted to this enemy. Must not be null
    //  numStacks: number of stacks applied to enemy when doing immediate damage >= 0
    //  Post: damage AND poison will be applied to enemy
    public override void poisonDamage(float initDmg, IVial poison, int numStacks) {
        Debug.Assert(initDmg >= 0.0f && numStacks >= 0);
        Debug.Assert(poison != null);

        // Change poison and start poison DoT loop if it hadn't started already
        lock(poisonLock) {
            // If you were not poisoned before, trigger poisoned event
            if (currentPoison == null) {
                unitPoisonedEvent.Invoke();
            }

            // Edit poison variables: if unit was already volatile with another poison, poison cannot be overriden
            if (!isVolatile) {
                currentPoison = poison;
            }
            poisonTimer = 0f;
            numPoisonStacks = Mathf.Min(numPoisonStacks + numStacks, MAX_STACKS);

            // Display poison
            if (poisonStackDisplay != null) {
                poisonStackDisplay.displayNumber(numPoisonStacks);
                poisonStackDisplay.displayColor(currentPoison.getColor());
            }

            // Enable enemy aura if aura is allowed to be present
            if (enemyAura != null && currentPoison.isEnemyAuraPresent(numPoisonStacks)) {
                enemyAura.setCaskPoison(currentPoison);
                enemyAura.setActive(true);

            } else if (enemyAura != null) {
                enemyAura.setActive(false);
            }

            // Run poison sequence if none are running at this point
            if (poisonDotRoutine == null && gameObject.activeInHierarchy) {
                poisonDotRoutine = StartCoroutine(poisonDotLoop());
            }

            // Check if enemy was made volatile
            if (volatileSequence == null && !isVolatile && gameObject.activeInHierarchy) {
                float volatileDuration;
                if (poison.makesTargetVolatile(out volatileDuration)) {
                    volatileSequence = StartCoroutine(volatileSideEffect(volatileDuration));
                }
            }

            // Edit footsteps audio
            float currentModifier = currentPoison.getStackSlowness(numPoisonStacks) * movementSpeedFactor;
            if (statusDisplay != null) {
                statusDisplay.displaySpeedStatus(currentModifier);
            }
            enemyAudio.setStepRateFactor(currentModifier);
        }

        // Do damage
        damage(initDmg, false);
    }


    // Main method to do poison damage (specific to twitch damage)
    //  initDmg: initial, immediate damage applied to enemy, > 0
    //  poison: PoisonVial that will be inflicted to this enemy IFF unit isn't already inflicted with poison
    //  numStacks: number of stacks applied to enemy when doing immediate damage
    //  Post: damage AND poison will be applied to enemy
    public override void weakPoisonDamage(float initDmg, IVial poison, int numStacks) {

        // Edit poison variables
        lock(poisonLock) {
            // Change poison ONLY IF you were not poisoned
            if (currentPoison == null) {
                unitPoisonedEvent.Invoke();
                currentPoison = poison;
            }

            // Change poison stacks
            poisonTimer = 0f;
            numPoisonStacks = Mathf.Min(numPoisonStacks + numStacks, MAX_STACKS);

            // Set poison display
            if (poisonStackDisplay != null) {
                poisonStackDisplay.displayNumber(numPoisonStacks);
                poisonStackDisplay.displayColor(currentPoison.getColor());
            }

            // Enable enemy aura if aura is allowed to be present
            if (enemyAura != null && currentPoison.isEnemyAuraPresent(numPoisonStacks)) {
                enemyAura.setCaskPoison(currentPoison);
                enemyAura.setActive(true);

            } else if (enemyAura != null) {
                enemyAura.setActive(false);
            }

            // Update poisonDoT Sequence if updated
            if (poisonDotRoutine == null && gameObject.activeInHierarchy) {
                poisonDotRoutine = StartCoroutine(poisonDotLoop());
            }

            // Check if enemy was made volatile
            if (volatileSequence == null && !isVolatile && gameObject.activeInHierarchy) {
                float volatileDuration;
                if (poison.makesTargetVolatile(out volatileDuration)) {
                    volatileSequence = StartCoroutine(volatileSideEffect(volatileDuration));
                }
            }

            // Edit footsteps audio
            float currentModifier = currentPoison.getStackSlowness(numPoisonStacks) * movementSpeedFactor;
            if (statusDisplay != null) {
                statusDisplay.displaySpeedStatus(currentModifier);
            }
            enemyAudio.setStepRateFactor(currentModifier);
        }

        // Do damage
        damage(initDmg, false);
    }


    // Main function to contaminate the unit with the poison they already have
    //  Pre: none
    //  Post: enemy suffers from severe burst damage
    public override void contaminate() {
        if (!isVolatile) {
            // Get necessary information to avoid synch errors
            IVial tempVial = null;
            int tempStacks = 0;

            lock (poisonLock) {
                tempVial = currentPoison;
                tempStacks = numPoisonStacks;
            }

            // Apply damage
            if (tempVial != null) {
                damage(tempVial.getContaminateDamage(tempStacks), false);

                // Apply aura damage if possible
                if (enemyAura != null) {
                    tempVial.applyEnemyAuraEffects(enemyAura, AuraType.RADIOACTIVE_EXPUNGE, tempStacks);
                }
            }
        }
    }


    // Private helper function to do death sequence
    //  Pre: possibleLoot is not empty
    private IEnumerator death() {
        Debug.Assert(possibleLoot.Length != 0);

        // Invoke death event
        unitDeathEvent.Invoke(this);
        
        // Only drop loot if this unit drops loot
        dropLoot();

        // Disable game related states except audio
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        enemyAudio.setFootstepsActive(false);
        enemyAudio.playDeathSound();

        if (statusDisplay != null) {
            statusDisplay.clear();
        }

        yield return new WaitForSeconds(1f);

        gameObject.SetActive(false);
    }

    
    // Main protected helpr function to drop loot IFF unit has loot to drop
    protected void dropLoot() {
        if (possibleLoot.Length > 0) {
            int numLoot = Random.Range(minLootDrops, maxLootDrops + 1);

            for (int l = 0; l < numLoot; l++) {
                Vector3 lootPosition = transform.position;
                Transform currentLoot = possibleLoot[Random.Range(0, possibleLoot.Length)].transform;
                Transform curLootInstance = Object.Instantiate(currentLoot, lootPosition, Quaternion.identity);
                curLootInstance.GetComponent<Loot>().setSpawnRadius(minLootDistance, maxLootDistance);
            }
        }
    }

    // Main function to reset unit, especially when player dies
    //  Pre: none
    //  Post: If enemy, reset to passive state, not sensing any enemies
    //        If player, reset all cooldowns to 0 and lose collectibles upon death
    public override void reset() {
        transform.position = spawnPoint;
        gameObject.SetActive(true);
        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;

        clearPoison();
        clearStun();
        movementSpeedFactor = 1f;
        armorMultiplier = 1.0f;
        attackMultiplier = 1.0f;

        // Reset volatility
        if (volatileSequence != null) {
            StopCoroutine(volatileSequence);
            volatileSequence = null;
        }
        isVolatile = false;

        // reset manic
        manic = false;

        // Reset health variables
        lock (healthLock) {
            curHealth = maxHealth;

            if (healthBar != null) {
                healthBar.setStatus(curHealth, maxHealth);
            }
        }

        statusEffectVFXs.clear();

        enemyResetEvent.Invoke();
    }


    // Main private helper function to clear all poison
    //  Pre:
    private void clearPoison() {
        // Reset poison variables
        lock (poisonLock) {
            if (poisonDotRoutine != null) {
                StopCoroutine(poisonDotRoutine);
                poisonDotRoutine = null;
            }

            // Only invoke event if currentPoison not null before
            if (currentPoison != null) {
                unitCurePoisonEvent.Invoke();
            }

            numPoisonStacks = 0;
            currentPoison = null;
            poisonTimer = 0f;

            if (poisonStackDisplay != null) {
                poisonStackDisplay.displayNumber(numPoisonStacks);
            }

            if (enemyAura != null) {
                enemyAura.setActive(false);
            }

            // display UI
            if (statusDisplay != null) {
                statusDisplay.displaySpeedStatus(movementSpeedFactor);
            }
        }
    }


    // Main function to check if a unit is poisoned
    //  Pre: none
    //  Post: returns whether or not the unit is poisoned
    public override bool isPoisoned() {
        bool poisoned;

        lock(poisonLock) {
            poisoned = numPoisonStacks > 0;
        }

        return poisoned && !isVolatile;
    }


    // Main function to handle spwning in of the unit (Player enters the game or enemies spawn in the area)
    //  Pre: none
    //  Post: spawns the enemy in IFF not spawned in game yet
    public override void spawnIn() {
        transform.position = spawnPoint;
        
        if (spawnInEffect != null) {
            spawnInEffect.activateEffect(transform.position, transform.position, spawnInTime);
        } else {
            base.spawnIn();
        }
    }

    // Main event handler for when spawn in finished
    private void onSpawnInEffectFinished() {
        base.spawnIn();
    }


    // Function to set movement to true 
    //  Pre: bool representing whether the player is moving or not
    //  Post: enact effects that happen while you're moving or deactivate effects when you aren't
    public override void setMoving(bool isMoving) {}
}
