using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LootDelegate : UnityEvent<Loot> {}

public class LootDropManager : MonoBehaviour
{
    // Private instance variables
    private HashSet<Loot> nearbyLoot;
    private Loot targetedLoot;
    private Coroutine loopingCheck;
    private const float proximityCheckDuration = 0.2f;
    private object lootLock = new object();

    [SerializeField]
    private ITwitchInventory inventory;
    [SerializeField]
    private IngredientDisplay tgtLootDisplay;

    // Other modules
    [Header("Other input modules")]
    [SerializeField]
    private UserInterfaceInputModule uiModule;

    // Public UnityEvent for when TargetedLoot changes
    public LootDelegate targetedLootChangeEvent;


    // On awake initialize HashSet
    private void Awake() {
        if (inventory == null) {
            Debug.LogError("Loot Drop Manager not connected to Inventory");
        }

        if (uiModule == null) {
            Debug.LogError("Loot drop manager not connected to UI Manager: cannot handle case for pausing");
        }

        if (tgtLootDisplay == null) {
            Debug.LogWarning("Loot Drop Manager not connected to targeted loot display: will not show info of nearest loot");
        }

        nearbyLoot = new HashSet<Loot>();
        targetedLootChangeEvent = new LootDelegate();
    }


    // Main IEnumerator to keep checking loot Proximity as long as there are nearby objects
    //  Pre: none
    //  Post: recalculates the nearest loot every proximityCheckDuration seconds
    private IEnumerator lootProximityCheck() {

        // Set up variables
        bool lootNearby = true;
        WaitForSeconds waitCheck = new WaitForSeconds(proximityCheckDuration);

        // Main loop that keeps running until no more nearby loot found
        while (lootNearby) {
            // Wait for wait check
            yield return waitCheck;

            // Get targeted loop and check if it's previous. If not, invoke targetedLoot change event
            Loot prevTargetedLoot = targetedLoot;
            targetedLoot = getNearestLoot();

            if (prevTargetedLoot != targetedLoot) {
                targetedLootChangeEvent.Invoke(targetedLoot);

                // update display
                updateIngredientDisplay();
            }

            // Check if loot is still nearby
            lock (lootLock) {
                lootNearby = nearbyLoot.Count > 0;
            }
        }

        // Indicate that no lootCheck is running right now
        loopingCheck = null;
    }


    // Private helper function to get nearest loot
    //  Pre: lootLock MUST be open
    //  Post: returns the nearest loot available. If no nearest loot available, return null
    private Loot getNearestLoot() {
        // Set variables
        Loot nearestLoot = null;
        float minDistance = float.MaxValue;

        // Grab lock to ensure no loot is being taken during iteration
        lock (lootLock) {

            // Go through each loot and pick the one with the minimum distance
            foreach (Loot loot in nearbyLoot) {
                float curDistance = Vector3.Distance(loot.transform.position, transform.position);
                
                if (curDistance < minDistance) {
                    minDistance = curDistance;
                    nearestLoot = loot;
                }
            }
        }

        return nearestLoot;
    }


    // Public method to collect the targeted loot (the closest loot available)
    //  Pre: inv != null
    //  Post: collects the loot and returns if collection is successful
    public bool collectTargetedLoot(ITwitchInventory inv) {
        if (targetedLoot == null) {
            return false;
        }
        
        // Collect loot
        bool success = targetedLoot.onPlayerCollect(inv);

        // If successsful, remove it from nearbyLoot and change targetedLoot
        if (success) {
            lock (lootLock) {
                nearbyLoot.Remove(targetedLoot);
            }

            targetedLoot = getNearestLoot();
            updateIngredientDisplay();
        }

        return success;
    }


    // Public method to quick craft the targeted loot if possible (the closest loot available)
    //  Pre: inv != null, isPrimary refers to either primary vial or secondary vial
    //  Post: collects the loot and returns if collection is successful
    public bool quickCraftTargetedLoot(ITwitchInventory inv, bool isPrimary) {
        if (targetedLoot == null) {
            return false;
        }

        // Quick craft loot
        bool success = targetedLoot.onPlayerQuickCraft(inv, isPrimary);

        // If successsful, remove it from nearbyLoot and change targetedLoot
        if (success) {
            lock (lootLock) {
                nearbyLoot.Remove(targetedLoot);
            }

            targetedLoot = getNearestLoot();
            updateIngredientDisplay();
        }

        return success;
    }


    // OnTriggerEnter event handler, if Loot enters trigger zone, add it to hashset
    public void OnTriggerEnter(Collider collider){
        Loot testLoot = collider.GetComponent<Loot>();

        if (testLoot != null) {
            lock (lootLock) {
                nearbyLoot.Add(testLoot);
            }

            // If no looping check running, activate looping check and set this testLoot as targeted (it's the first one)
            if (loopingCheck == null) {
                targetedLoot = testLoot;
                updateIngredientDisplay();
                loopingCheck = StartCoroutine(lootProximityCheck());
            }
        }
    }


    // Main private helper function to update the ingredient display
    private void updateIngredientDisplay() {
        // Only go in here when the display exists
        if (tgtLootDisplay != null) {
            // Get ingredient from loot and display it using display
            Ingredient displayedIng = (targetedLoot == null) ? null : targetedLoot.getIngredient();
            tgtLootDisplay.displayIngredient(displayedIng);
        }
    }


    // OnTriggerEnter event handler, if Loot enters trigger zone, add it to hashset
    public void OnTriggerExit(Collider collider){
        Loot testLoot = collider.GetComponent<Loot>();

        if (testLoot != null && nearbyLoot.Contains(testLoot)) {
            lock (lootLock) {
                nearbyLoot.Remove(testLoot);
            }
        }
    }

    // Event handler function for when player presses collect
    public void onPickUpPress(InputAction.CallbackContext value) {
        if (value.started && !uiModule.inMenu()) {
            collectTargetedLoot(inventory);
        }
    }


    // Event handler function for when player presses collect
    public void onPrimaryCraftPress(InputAction.CallbackContext value) {
        if (value.started && !uiModule.inMenu()) {
            quickCraftTargetedLoot(inventory, true);
        }
    }


    // Event handler function for when player presses collect
    public void onSecondaryCraftPress(InputAction.CallbackContext value) {
        if (value.started && !uiModule.inMenu()) {
            quickCraftTargetedLoot(inventory, false);
        }
    }
}
