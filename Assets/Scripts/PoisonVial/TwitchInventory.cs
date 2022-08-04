using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

public class TwitchInventory : ITwitchInventory
{
    // Private instance variables
    private IVial primaryVial = null;
    private IVial secondaryVial = null;
    private Dictionary<Ingredient, int> ingredientInventory;
    private Dictionary<IVial, Coroutine> vialUltCooldownManager;

    private readonly object ingredientsLock = new object();
    private readonly object vialLock = new object();

    // UI Elements to signal (Modularize ITwitchPlayerUI? This is also pointed to by PlayerStatus)
    [Header("UI Elements")]
    [SerializeField]
    private ITwitchPlayerUI mainPlayerUI;
    [SerializeField]
    private TextPopup upgradePopup;
    [SerializeField]
    private float upgradeInterval = 1.0f;
    public UnityEvent playerCraftEvent;


    [Header("Audio Elements")]
    [SerializeField]
    private TwitchPlayerAudio playerAudio;

    // Player Aura Elements
    [SerializeField]
    private EnemyAura playerAura;
    private Coroutine currentAuraSequence = null;

    // Variables to keep track of previous craft
    private bool gainedSideEffect = false;



    // On awake, initialize backend instance variables
    private void Start() {
        // Initialize variables
        ingredientInventory = new Dictionary<Ingredient, int>();
        vialUltCooldownManager = new Dictionary<IVial, Coroutine>();

        // Display UI
        mainPlayerUI.displaySecondaryVial(secondaryVial);
        mainPlayerUI.displayPrimaryVial(primaryVial);
        mainPlayerUI.displayCraftingTimer(0.0f, 10.0f, false);

        // Error check
        if (mainPlayerUI == null) {
            Debug.LogError("Inventory not connected to main player UI");
        }

        // If no player aura, say warning. else, connect that to vial eecution event
        if (playerAura == null) {
            Debug.LogWarning("No PlayerAura connected to inventory. Player Aura side effects may not be active");
        } else {
            playerAura.targetKilledEvent.AddListener(onVialExecution);
        }
    }


    // IEnumerator sequence for when player aura effect is active
    //  It will stop on its own if the vial changes during the sequence
    private IEnumerator poisonVialAuraSequence() {

        // Main loop for doing damage
        bool playerAuraActive;
        lock (vialLock) {
            playerAuraActive = (primaryVial != null && primaryVial.isPlayerAuraPresent());
        }

        while (playerAuraActive) {
            // wait for aura tick time
            yield return new WaitForSeconds(primaryVial.getAuraRate());

            // Get aura status
            lock (vialLock) {
                playerAuraActive = (primaryVial != null && primaryVial.isPlayerAuraPresent());

                // Do damage to all nearby enemies IFF conditions still apply
                if (playerAuraActive) {
                    primaryVial.applyEnemyAuraEffects(playerAura, AuraType.NO_TOUCHING, 0);
                }  
            }
        }

        currentAuraSequence = null;
    }


    // Main function to utilize player aura 
    //  Pre: auraType is one of the aura types listed in the enum (VirtualSideEffect.cs)
    //  Post: Uses player aura for specific aura type
    public override void utilizePlayerAura(AuraType auraType) {
        
        // See if player aura is active, if so, do aura effects
        bool playerAuraActive;
        lock (vialLock) {
            playerAuraActive = (primaryVial != null && primaryVial.isPlayerAuraPresent());

            if (playerAuraActive) {
                primaryVial.applyEnemyAuraEffects(playerAura, auraType, 0);
            }
        }
    }


    // Main function to add an Ingredient to the current inventory
    //  Pre: ing != null 
    //  Post: Returns whether or not successful. If so, ingredient will be added in the inventory
    public override bool addIngredient(Ingredient ing) {
        Debug.Assert(ing != null);

        lock(ingredientsLock) {
            // If not found in Inventory, add it with a number of 0
            if (!ingredientInventory.ContainsKey(ing)) {
                ingredientInventory.Add(ing, 0);
            }

            // Increment count
            ingredientInventory[ing]++;
        }

        playerAudio.playPickUpSound();
        return true;
    }


    // Main function to remove ingredient from the current inventory
    //  Pre: ing != null
    //  Post: If removal is successful. If true, inventory removes 1 instance of ingredient from inventory
    public override bool removeIngredient(Ingredient ing) {
        Debug.Assert(ing != null);
        bool removalSuccess;

        lock (ingredientsLock) {
            // Successful removal is dependent on whether or not you have at least 1 instance in inventory
            removalSuccess = ingredientInventory.ContainsKey(ing);

            // If ingredient found in inventory, decrement
            if (removalSuccess) {
                ingredientInventory[ing]--;

                // If you have no more instances of ingredient anymore, remove it
                if (ingredientInventory[ing] <= 0) {
                    ingredientInventory.Remove(ing);
                }
            }
        }

        return removalSuccess;
    }


    // Main accessor function to get the Primary Vial from this inventory
    //  Pre: none
    //  Post: Access a primary vial that CAN be null (means that you had an empty vial)
    public override IVial getPrimaryVial() {
        return primaryVial;
    }


    // Main function to decrement primary vial count
    //  Pre: ammo >= 0
    //  Post: returns true if successful. If so, primary vial ammo count gets decremented
    public override bool consumePrimaryVial(int ammo) {
        Debug.Assert(ammo >= 0);
        bool success;

        lock (vialLock) {
            if (primaryVial == null) {
                success = false;
            } else {
                success = primaryVial.useVial(ammo);

                if (primaryVial.getAmmoLeft() <= 0) {
                    // Get rid of cooldown IF IT EXIST
                    if (vialUltCooldownManager.ContainsKey(primaryVial)) {
                        StopCoroutine(vialUltCooldownManager[primaryVial]);
                        vialUltCooldownManager.Remove(primaryVial);
                    }

                    // Set primary vial to null and invoke event handler
                    primaryVial = null;
                    onPrimaryVialChange();
                }
            }

            mainPlayerUI.displayPrimaryVial(primaryVial);
        }

        return success;
    }


    // Main function to swap vials
    //  Pre: none 
    //  Post: primary vial will become secondary vial and vice versa
    public override void swapVials() {
        lock (vialLock) {
            IVial tempVial = primaryVial;
            primaryVial = secondaryVial;
            secondaryVial = tempVial;

            // Display UI
            mainPlayerUI.displaySecondaryVial(secondaryVial);
            mainPlayerUI.displayPrimaryVial(primaryVial);
        }

        onPrimaryVialChange();
    }


    // Main function to clear out inventory upon death
    //  Pre: None
    //  Post: Clears Inventory of everything
    public override void clear() {
        // Clear Ingredient inventory
        lock (ingredientsLock) {
            ingredientInventory.Clear();
        }

        // Clear poison vials
        lock (vialLock) {
            primaryVial = null;
            secondaryVial = null;

            // Display UI
            mainPlayerUI.displaySecondaryVial(secondaryVial);
            mainPlayerUI.displayPrimaryVial(primaryVial);
        }

        // Clear ultimates
        foreach (KeyValuePair<IVial, Coroutine> entry in vialUltCooldownManager) {
            StopCoroutine(entry.Value);
        }
        vialUltCooldownManager.Clear();

        // On primary vial change
        onPrimaryVialChange();
    }


    // Main helper function for handling the event for when the primary vial changed
    private void onPrimaryVialChange() {
        // Check if player aura is active because of upgrade
        if (playerAura != null) {

            // If conditions are met AND no running coroutine at this moment
            if (primaryVial != null && primaryVial.isPlayerAuraPresent() && currentAuraSequence == null) {
                playerAura.setCaskPoison(primaryVial);
                playerAura.setActive(true);
                currentAuraSequence = StartCoroutine(poisonVialAuraSequence());

            // If conditions are not met, deactivate aura
            } else if (primaryVial == null || !primaryVial.isPlayerAuraPresent()) {
                playerAura.setActive(false);
            }
        }

        // Update Ultimate icon
        if (primaryVial != null && !vialUltCooldownManager.ContainsKey(primaryVial)) {
            mainPlayerUI.updateUltCooldown(0f, 1f);
        }

    }


    // Main function to upgrade primary vial with one ingredient
    //  Pre: Ing != null
    //  Post: Returns a bool that says if its successful. If so, primary vial gets upgraded
    public override bool upgradePrimaryVial(Ingredient ing, bool inMenu) {
        Debug.Assert(ing != null);
        bool success;

        lock (vialLock) {
            if (primaryVial == null) {
                primaryVial = new PoisonVial(ing, this);
                success = true;
                gainedSideEffect = false;
            } else {
                bool hadSideEffect = primaryVial.hasSideEffect();
                success = primaryVial.upgrade(ing);
                gainedSideEffect = hadSideEffect != primaryVial.hasSideEffect();
            }
        }

        // If successful and UnityEvent != null, trigger event
        if (success && playerCraftEvent != null) {
            playerCraftEvent.Invoke();
        } else if (playerCraftEvent == null) {
            mainPlayerUI.displayPrimaryVial(primaryVial);
            onPrimaryVialChange();
        }

        // Display error message if quick crafting and not successful
        if (!success && !inMenu) {
            mainPlayerUI.displayQuickCraftingError();
        }

        return success;
    }


    // Main function to upgrade primary vial with one ingredient
    //  Pre: Ing1 != null && ing2 != null
    //  Post: Returns a bool that says if its successful. If so, primary vial gets upgraded
    public override bool upgradePrimaryVial(Ingredient ing1, Ingredient ing2) {
        Debug.Assert(ing1 != null && ing2 != null);
        bool success;

        lock (vialLock) {
            if (primaryVial == null) {
                primaryVial = new PoisonVial(ing1, ing2, this);
                success = true;
                gainedSideEffect = primaryVial.hasSideEffect();
            } else {
                bool hadSideEffect = primaryVial.hasSideEffect();
                success = primaryVial.upgrade(ing1, ing2);
                gainedSideEffect = hadSideEffect != primaryVial.hasSideEffect();
            }
        }

        // If successful and UnityEvent != null, trigger event
        if (success && playerCraftEvent != null) {
            playerCraftEvent.Invoke();
        } else if (playerCraftEvent == null) {
            mainPlayerUI.displayPrimaryVial(primaryVial);
            onPrimaryVialChange();
        }

        return success;
    }


    // Main function to upgrade primary vial with one ingredient
    //  Pre: at least 1 of the ingredients is not null
    //  Post: Returns a bool that says if its successful. If so, primary vial gets replaced by new vial made by 2 ingredients
    public override bool replacePrimaryVial(Ingredient ing1, Ingredient ing2) {
        Debug.Assert(ing1 != null || ing2 != null);

        // If only 1 ingredient is non null, use the single constructor
        if (ing1 == null || ing2 == null) {
            Ingredient currentIng = (ing1 != null) ? ing1 : ing2;
            primaryVial = new PoisonVial(currentIng, this);
        } else {
            primaryVial = new PoisonVial(ing1, ing2, this);
        }

        gainedSideEffect = primaryVial.hasSideEffect();

        // If successful and UnityEvent != null, trigger event
        if (playerCraftEvent != null) {
            playerCraftEvent.Invoke();
        } else {
            mainPlayerUI.displayPrimaryVial(primaryVial);
            onPrimaryVialChange();
        }

        return true;
    }


    // Main function to upgrade secondary vial with one ingredient
    //  Pre: Ing != null, inMenu is simply a flag that says if you are in a menu or not (defaulted to false)
    //  Post: Returns a bool that says if its successful. If so, secondary vial gets upgraded
    public override bool upgradeSecondaryVial(Ingredient ing, bool inMenu) {
        Debug.Assert(ing != null);
        bool success;

        lock (vialLock) {
            if (secondaryVial == null) {
                secondaryVial = new PoisonVial(ing, this);
                success = true;
                gainedSideEffect = false;
            } else {
                bool hadSideEffect = secondaryVial.hasSideEffect();
                success = secondaryVial.upgrade(ing);
                gainedSideEffect = hadSideEffect != secondaryVial.hasSideEffect();
            }
        }

        // If successful and UnityEvent != null, trigger event
        if (success && playerCraftEvent != null) {
            playerCraftEvent.Invoke();
        } else if (playerCraftEvent == null) {
            mainPlayerUI.displaySecondaryVial(secondaryVial);
        }

        // Display error if you're quick crafting
        if (!success && !inMenu) {
            mainPlayerUI.displayQuickCraftingError();
        }

        return success;
    }


    // Main function to upgrade primary vial with one ingredient
    //  Pre: Ing1 != null && ing2 != null
    //  Post: Returns a bool that says if its successful. If so, secondary vial gets upgraded
    public override bool upgradeSecondaryVial(Ingredient ing1, Ingredient ing2) {
        Debug.Assert(ing1 != null && ing2 != null);
        bool success;

        lock (vialLock) {
            if (secondaryVial == null) {
                secondaryVial = new PoisonVial(ing1, ing2, this);
                success = true;
                gainedSideEffect = secondaryVial.hasSideEffect();
            } else {
                bool hadSideEffect = secondaryVial.hasSideEffect();
                success = secondaryVial.upgrade(ing1, ing2);
                gainedSideEffect = hadSideEffect != secondaryVial.hasSideEffect();
            }

            mainPlayerUI.displaySecondaryVial(secondaryVial);
        }

        // If successful and UnityEvent != null, trigger event
        if (success && playerCraftEvent != null) {
            playerCraftEvent.Invoke();
        } else if (playerCraftEvent == null) {
            mainPlayerUI.displaySecondaryVial(secondaryVial);
        }

        return success;
    }


    // Main function to upgrade secondary vial with one ingredient
    //  Pre: at least 1 of the ingredients is not null
    //  Post: Returns a bool that says if its successful. If so, secondary vial gets replaced by new vial made by 2 ingredients
    public override bool replaceSecondaryVial(Ingredient ing1, Ingredient ing2) {
        Debug.Assert(ing1 != null || ing2 != null);

        // If only 1 ingredient is non null, use the single constructor
        if (ing1 == null || ing2 == null) {
            Ingredient currentIng = (ing1 != null) ? ing1 : ing2;
            secondaryVial = new PoisonVial(currentIng, this);
        } else {
            secondaryVial = new PoisonVial(ing1, ing2, this);
        }

        gainedSideEffect = secondaryVial.hasSideEffect();

        // If successful and UnityEvent != null, trigger event
        if (playerCraftEvent != null) {
            playerCraftEvent.Invoke();
        } else {
            mainPlayerUI.displaySecondaryVial(secondaryVial);
        }

        return true;
    }


    // Main function to display ingredients given an array of ingredient icons
    //  Pre: array of icons != null with non-null elements AND array length >= number of distinct ingredient types
    //  Post: Displays ingredients onto ingredient icons
    public override void displayIngredients(IngredientIcon[] ingredientIcons) {
        // Pre condition
        Debug.Assert(ingredientIcons != null && ingredientIcons.Length >= ingredientInventory.Count);

        // Index used to access the icons array
        int iconIndex = 0;

        // Iterate over each entry within the dictionary
        foreach (KeyValuePair<Ingredient, int> entry in ingredientInventory) {
            // Set up each ingredient icon
            IngredientIcon currentIcon = ingredientIcons[iconIndex];
            Debug.Assert(currentIcon != null);
            currentIcon.SetUpIcon(entry.Key, entry.Value);

            // Increment iconIndex
            iconIndex++;
        }

        // You end on the last iconIndex that's empty, clear out all remaining icons
        for (int i = iconIndex; i < ingredientIcons.Length; i++) {
            IngredientIcon currentIcon = ingredientIcons[i];
            Debug.Assert(currentIcon != null);
            currentIcon.ClearIcon();
        }
    }


    // Main function to display secondary vial in an icon
    //  Pre: VialIcon must not be null
    //  Post: VialIcon will now display secondaryVial
    public override void displaySecondaryVial(VialIcon vialIcon) {
        vialIcon.DisplayVial(secondaryVial);
    }


    // Main function to check if you can upgrade a vial
    //  Pre: Ing1 and Ing2 are ingredients used to upgrade (can be null) and isPrimary is a bool: true - upgrade primary, false - upgrade secondary
    //  Post: returns a bool to check if you can really upgrade, if vial is null, returns true immediately
    public override bool canUpgradeVial(Ingredient ing1, Ingredient ing2, bool isPrimary) {
        // Get the target vial
        IVial targetVial = (isPrimary) ? primaryVial : secondaryVial;
        
        // If null, return true immediately
        if (targetVial == null) {
            return true;
        }

        int projectedStat = targetVial.getCurrentTotalStat();
        projectedStat += (ing1 == null) ? 0 : ing1.getNumStatContribution();
        projectedStat += (ing2 == null) ? 0 : ing2.getNumStatContribution();
 
        return projectedStat <= targetVial.getMaxTotalStat();
    }


    // Main function to add craft event listener
    //  Pre: eventHandler does not equal null
    //  Post: eventHandler will listen to craftEvent of inventory. Will also initialize event if it has not been initialized yet
    public override void addCraftListener(UnityAction eventHandler) {
        if (playerCraftEvent == null) {
            playerCraftEvent = new UnityEvent();
        }

        playerCraftEvent.AddListener(eventHandler);
    }


    // Main function to run craftingSequence
    //  Pre: float to determin how long the crafting lasts > 0, NO CONCONCURRENT CRAFTING
    //  Post: runs crafting sequence
    public override IEnumerator craftSequence(float craftTime) {
        Debug.Assert(craftTime > 0.0f);

        // Timer sequence
        float timer = 0.0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();
        mainPlayerUI.displayCraftingTimer(0.0f, craftTime, true);
        playerAudio.setVialMixing(true);

        while (timer <= craftTime) {
            yield return waitFrame;

            timer += Time.deltaTime;
            mainPlayerUI.displayCraftingTimer(timer, craftTime, true);
        }

        // Player audio
        playerAudio.setVialMixing(false);
        playerAudio.playBasicUpgradeSound();
        if (gainedSideEffect) {
            playerAudio.playSideEffectUpgradeSound();
        }

        // Update all possible UI
        mainPlayerUI.displayCraftingTimer(0.0f, craftTime, false);
        mainPlayerUI.displaySecondaryVial(secondaryVial);
        mainPlayerUI.displayPrimaryVial(primaryVial);

        onPrimaryVialChange();
        StartCoroutine(displayUpgradeGains());
    }


    // Main function to do upgrade popup sequence
    //  Pre: none
    //  Post: displays the result of the previous upgrades via popups
    private IEnumerator displayUpgradeGains() {
        // Calculate wait frame
        WaitForSeconds upgradeIntervalFrame = new WaitForSeconds(upgradeInterval);

        // show primary vial upgrades
        if (primaryVial != null) {
            List<string> primaryUpgrades = primaryVial.getPrevUpgradeDisplays();

            foreach (string upgrade in primaryUpgrades) {
                TextPopup dmgPopup = Object.Instantiate(upgradePopup, transform.position, Quaternion.identity);
                dmgPopup.SetUpPopup(upgrade);
                yield return upgradeIntervalFrame;
            }
        }

        // show secondary vial upgrades
        if (secondaryVial != null) {
            List<string> secondaryUpgrades = secondaryVial.getPrevUpgradeDisplays();

            foreach (string upgrade in secondaryUpgrades) {
                TextPopup dmgPopup = Object.Instantiate(upgradePopup, transform.position, Quaternion.identity);
                dmgPopup.SetUpPopup(upgrade);
                yield return upgradeIntervalFrame;
            }
        }
    }


    // Main function to check if you can do your ultimate
    //  Pre: none
    //  Post: return if ult execution is successful, returns false otherwise
    public override bool willExecutePrimaryUltimate(ITwitchStatus player, Vector3 dest) {
        // Get reference to current primary vial
        IVial currentPrimaryVial;
        lock (vialLock) {
            currentPrimaryVial = primaryVial;
        }

        // Check if vial even has an ultimate
        if (currentPrimaryVial != null && currentPrimaryVial.hasUltimate()) {
            // Check if cooldown is NOT running (NOT found in cooldown manager) AND that you could even execute this ultimate
            if (!vialUltCooldownManager.ContainsKey(currentPrimaryVial) && currentPrimaryVial.executeUltimate(player, dest)) {
                // Update costs
                consumePrimaryVial(currentPrimaryVial.getUltimateCost());

                // Only run cooldown coroutine if primary vial doesn't become empty afterwards
                if (primaryVial != null) {
                    Coroutine currentCooldown = StartCoroutine(ultimateCooldownSequence(currentPrimaryVial.getUltimateCooldown(), currentPrimaryVial));
                    vialUltCooldownManager.Add(currentPrimaryVial, currentCooldown);
                }

                return true;
            } else {
                Debug.Log("Cannot use ability");
            }

        } else {
            Debug.Log("No ultimate found with current vial");
        }

        return false;
    }


    // Main Ult cooldown sequence to handle ultimates
    private IEnumerator ultimateCooldownSequence(float ultCooldown, IVial vial) {
        // Initialize timer
        float timer = 0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        // Timer loop
        while (timer < ultCooldown) {
            yield return waitFrame;

            // Update cooldown
            if (vial == primaryVial) {
                mainPlayerUI.updateUltCooldown(ultCooldown - timer, ultCooldown);
            }

            // Update timer
            timer += Time.fixedDeltaTime;
        }

        // Update cooldown
        if (vial == primaryVial) {
            mainPlayerUI.updateUltCooldown(0f, ultCooldown);
        }

        // Remove coroutine from dictionary
        vialUltCooldownManager.Remove(vial);
    }
}
