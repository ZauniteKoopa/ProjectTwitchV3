using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// Vial class
public class PoisonVial : IVial
{
    // Main stats
    private int potency;
    private int poison;
    private int reactivity;
    private int stickiness;

    private int currentTotalStats;
    private const int MAX_TOTAL_STATS = 10;
    private const int MAX_SINGLE_STAT = 5;

    // Ammo management
    private int ammo;
    private static int MAX_AMMO = 60;
    private static int ONE_ING_AMMO = 40;
    private static int TWO_ING_AMMO = 50;
    private static int AMMO_UPGRADE_AMOUNT = 10;

    // Colors for color mixing
    private static Color potentColor = Color.magenta;
    private static Color poisonColor = Color.green;
    private static Color reactiveColor = Color.yellow;
    private static Color stickinessColor = Color.cyan;
    private Color vialColor;

    // CSV Parsing variables
    private static bool csvParsed = false;
    private static string csvFileName = "BasePoisonVialData";

    //Constants for bullet damage (find a wat to make this editable from the designer)
    private static float BASE_DAMAGE;
    private static float DMG_GROWTH;

    //constants for poison stack damage
    private static float BASE_POISON;
    private static float POISON_GROWTH;
    private static float BASE_POISON_DECAY = 1.0f;

    //constants for contaminate damage
    private static float BASE_CONTAMINATE_DMG;
    private static float BASE_CON_GROWTH;
    private static float BASE_STACK_DMG;
    private static float STACK_DMG_GROWTH;

    //constants for stack slowness
    private static float BASE_SLOWNESS;
    private static float SLOWNESS_GROWTH;

    // Constants for cask slowness
    private static float BASE_CASK_SLOWNESS;
    private static float CASK_SLOWNESS_GROWTH;

    // Side Effect used
    private VirtualSideEffect sideEffect;
    private const int SIDE_EFFECT_UPGRADE_THRESHOLD = 3;
    private const int AURA_THRESHOLD = 4;


    // Main raw constructor for a poison vial
    //  Pre: initialAmmo > 0 and 0 <= all stats <= 5
    public PoisonVial(int pot, int poi, int r, int s, int initialAmmo) {
        Debug.Assert(initialAmmo > 0);
        Debug.Assert(pot >= 0 && pot <= 5);
        Debug.Assert(poi >= 0 && poi <= 5);
        Debug.Assert(r >= 0 && r <= 5);
        Debug.Assert(s >= 0 && s <= 5);

        if (!csvParsed) {
            Debug.LogError("Poison Vial base stats not set up, did you forget a loader?");
        }

        potency = pot;
        poison = poi;
        reactivity = r;
        stickiness = s;
        currentTotalStats = pot + poi + r + s;

        ammo = initialAmmo;
        sideEffect = PoisonVialDatabase.getRandomSideEffect(Specialization.NONE);
        vialColor = calculateColor();
    }


    // Main constructor to craft a poison vial from only 1 ingredient
    public PoisonVial(Ingredient ing) {
        if (!csvParsed) {
            Debug.LogError("Poison Vial base stats not set up, did you forget a loader?");
        }

        potency = 0;
        poison = 0;
        reactivity = 0;
        stickiness = 0;
        currentTotalStats = 0;

        sideEffect = PoisonVialDatabase.getRandomSideEffect(Specialization.NONE);
        upgrade(ing);
        ammo = ONE_ING_AMMO;
    }


    // Main constructor to craft a poison vial from only 1 ingredient
    public PoisonVial(Ingredient ing1, Ingredient ing2) {
        if (!csvParsed) {
            Debug.LogError("Poison Vial base stats not set up, did you forget a loader?");
        }

        potency = 0;
        poison = 0;
        reactivity = 0;
        stickiness = 0;
        currentTotalStats = 0;

        sideEffect = PoisonVialDatabase.getRandomSideEffect(Specialization.NONE);
        upgrade(ing1, ing2);
        ammo = TWO_ING_AMMO;
    }


    // Private helper function to parse the CSV
    //  Pre: none
    //  Post: static variables have been updated to reflect csv values
    public static void parseBaseVialCSV() {
        // Only load if csv wasn't parsed
        if (!csvParsed) {
            // Load CSV text file
            TextAsset csvFile = Resources.Load<TextAsset>("DesignerCSVs/" + csvFileName);
            if (csvFile == null) {
                Debug.LogError("Resources/DesignerCSVs/" + csvFileName + " not found to set Vial stats");
            }

            // Parse out rows and then get the number of rows that must be considered (found in first row)
            string[] csvRows = csvFile.text.Split('\n');
            string[] firstRow = csvRows[0].Split(',');
            int numRowsConsidered = int.Parse(firstRow[1]);

            // Go through the list accordingly
            for (int r = 0; r < numRowsConsidered; r++) {
                // Parse pow
                int rowIndex = 2 + r;
                string[] currentRow = csvRows[rowIndex].Split(',');

                // Get information from row
                string currentAttribute = currentRow[0];
                float curRowBase = float.Parse(currentRow[1]);
                float curRowGrowth = float.Parse(currentRow[2]);

                // Connect the information to current stats based on attributes
                switch(currentAttribute) {
                    case "BoltDamage":
                        BASE_DAMAGE = curRowBase;
                        DMG_GROWTH = curRowGrowth;
                        break;
                    case "Poison DoT":
                        BASE_POISON = curRowBase;
                        POISON_GROWTH = curRowGrowth;
                        break;
                    case "BaseContaminate":
                        BASE_CONTAMINATE_DMG = curRowBase;
                        BASE_CON_GROWTH = curRowGrowth;
                        break;
                    case "ContaminateStackGrowth":
                        BASE_STACK_DMG = curRowBase;
                        STACK_DMG_GROWTH = curRowGrowth;
                        break;
                    case "StackSlowness":
                        BASE_SLOWNESS = curRowBase;
                        SLOWNESS_GROWTH = curRowGrowth;
                        break;
                    case "CaskSlowness":
                        BASE_CASK_SLOWNESS = curRowBase;
                        CASK_SLOWNESS_GROWTH = curRowGrowth;
                        break;
                    default:
                        Debug.LogError("No matching attribute on row " + (rowIndex + 1) + " of Resources/DesignerCSVs/" + csvFileName);
                        break;
                }
            }

            // Collect ammo info
            parseAmmoInformation(numRowsConsidered, csvRows);

            csvParsed = true;
        }
    }


    // Private helper function to parse ammo information
    //  Pre: numStatRowsConsidered >= 0 and csvRows.Length >= numStatRowsConsidered + 6
    //  Post: static ammo variables are set
    private static void parseAmmoInformation(int numStatRowsConsidered, string[] csvRows) {
        Debug.Assert(numStatRowsConsidered >= 0 && csvRows.Length >= numStatRowsConsidered + 6);

        for (int r = 0; r < 4; r++) {
            // Parse pow
            int rowIndex = 2 + numStatRowsConsidered + r;
            string[] currentRow = csvRows[rowIndex].Split(',');
            int curRowNumber = int.Parse(currentRow[1]);

            // Switch statement for ammo information
            switch (r) {
                case 0:
                    ONE_ING_AMMO = curRowNumber;
                    break;
                
                case 1:
                    TWO_ING_AMMO = curRowNumber;
                    break;
                
                case 2:
                    AMMO_UPGRADE_AMOUNT = curRowNumber;
                    break;
                
                case 3:
                    MAX_AMMO = curRowNumber;
                    break;
                
                default:
                    Debug.LogError("Invalid row");
                    break;
            }

        }
    }


    // Main function to use vial with given ammo cost
    //  Pre: ammoCost > 0
    //  Post: returns true if successful and decrements ammo accordingly, returns false if not enough ammo.
    public bool useVial(int ammoCost) {
        Debug.Assert(ammoCost >= 0);

        if (ammoCost > ammo) {
            return false;
        }

        ammo -= ammoCost;
        return true;
    }


    // Function to get access to how much ammo left for UI displays
    //  Post: returns how much ammo left. Can be negative
    public int getAmmoLeft() {   
        return ammo;
    }


    // Function to get access to the max vial size (It's a constant)
    public static int getMaxVialSize() {
        Debug.Assert(MAX_AMMO >= 0);
        return MAX_AMMO;
    }


    // Function to get access to how much immediate damage a bolt / bullet does
    //  Post: returns how much damage a bullet does based on current stats > 0
    public float getBoltDamage() {
        float boltDamage = BASE_DAMAGE + (DMG_GROWTH * potency);
        boltDamage *= sideEffect.boltDamageMultiplier();

        Debug.Assert(boltDamage >= 0.0f);
        return boltDamage;
    }

    
    // Function to get access to poison damage based on the number of stacks a unit has
    //  Pre: 0 < numStacks <= 5
    //  Post: returns the amount of poison damage unit suffers based on this vial's stats > 0
    public float getPoisonDamage(int numStacks) {
        Debug.Assert(numStacks > 0 && numStacks <= 6);

        float poisonDamage = (BASE_POISON + (POISON_GROWTH * poison)) * numStacks;

        Debug.Assert(poisonDamage >= 0.0f);
        return poisonDamage;
    }


    // Function to get the decay rate of this poison when dealing DoT to enemies
    //  Pre: none
    //  Post: a float that's greater than 0
    public float getPoisonDecayRate() {
        float decayRate = BASE_POISON_DECAY * sideEffect.decayRateMultiplier();

        Debug.Assert(decayRate > 0.0f);
        return decayRate;
    }


    // Function to calculate how much contaminate burst damage a unit suffers
    //  Pre: 0 < numStacks <= 5
    //  Post: returns burst damage based on number of stacks and vial's stats > 0
    public float getContaminateDamage(int numStacks) {
        Debug.Assert(numStacks > 0 && numStacks <= 6);

        float curBaseDmg = BASE_CONTAMINATE_DMG + (BASE_CON_GROWTH * reactivity);
        float stackDmg = BASE_STACK_DMG + (STACK_DMG_GROWTH * reactivity);
        float contaminateDamage = curBaseDmg + (stackDmg * (numStacks - 1));
        
        Debug.Assert(contaminateDamage >= 0.0f);
        return contaminateDamage;
    }


    // Function to get cask slowness speed factor
    //  Pre: none
    //  Post: 0 <= returnValue <= 1.0
    public float getCaskSlowness() {
        float caskSlowness = BASE_CASK_SLOWNESS + (CASK_SLOWNESS_GROWTH * stickiness);

        Debug.Assert(caskSlowness >= 0.0f && caskSlowness <= 1.0f);
        return caskSlowness;
    }


    // Function to calculate how much a unit is slowed by stacked poison
    //  Pre: 0 < numStacks <= 6
    //  Post: 0 < returnValue <= 1.0
    public float getStackSlowness(int numStacks) {
        Debug.Assert(numStacks > 0 && numStacks <= 6);

        float stackSlowness = (stickiness > 0) ? BASE_SLOWNESS + (SLOWNESS_GROWTH * (stickiness - 1)) : 1.0f;
        stackSlowness = sideEffect.modifyStackSpeedFactor(stackSlowness);
        stackSlowness = Mathf.Lerp(1.0f, stackSlowness, numStacks / 6f);

        Debug.Assert(stackSlowness > 0.0f && stackSlowness <= 1.0f);
        return stackSlowness;
    }


    // Function to calculate initial cask damage
    //  Pre: none
    //  Post: return value >= 0
    public float getInitCaskDamage() {
        return 2f * getBoltDamage();
    }


    // Function to get access to the base stats of this vial
    //  Pre: none
    //  Post: returns a dictionary with 4 fixed properties: "Poison", "Potency", "Reactivity", and "Stickiness"
    public Dictionary<string, int> getStats() {
        Dictionary<string, int> statDict = new Dictionary<string, int>();

        statDict.Add("Potency", potency);
        statDict.Add("Poison", poison);
        statDict.Add("Reactivity", reactivity);
        statDict.Add("Stickiness", stickiness);

        Debug.Assert(statDict.ContainsKey("Potency") && statDict.ContainsKey("Poison") && statDict.ContainsKey("Reactivity") && statDict.ContainsKey("Stickiness"));
        return statDict;
    }


    // Function to upgrade Poison using only one ingredient
    //  Pre: ing != null
    //  Post: Returns whether upgrade is successful. If successful, vial is updated with this ingredient
    public bool upgrade(Ingredient ing) {
        Debug.Assert(ing != null);

        if (currentTotalStats >= MAX_TOTAL_STATS) {
            return false;
        }

        // Calculate stat gains: make sure you either contribute all stats or just enough to hit 
        int numStats = Mathf.Min(Ingredient.NUM_STATS_CONTRIBUTED, MAX_TOTAL_STATS - currentTotalStats);

        // Increment by number of stats possible
        for (int i = 0; i < numStats; i++) {
            // Check if there's any stats to ignore when upgrading (reached single max)
            int ignoredIndex = -1;
            ignoredIndex = (potency >= MAX_SINGLE_STAT) ? 0 : ignoredIndex;
            ignoredIndex = (poison >= MAX_SINGLE_STAT) ? 1 : ignoredIndex;
            ignoredIndex = (reactivity >= MAX_SINGLE_STAT) ? 2 : ignoredIndex;
            ignoredIndex = (stickiness >= MAX_SINGLE_STAT) ? 3 : ignoredIndex;

            // Calculate stat gain
            int currentStatGain = ing.calculateStatGain(ignoredIndex);

            // Switch on stat gain
            switch(currentStatGain) {
                case 0:
                    potency++;
                    break;
                case 1:
                    poison++;
                    break;
                case 2:
                    reactivity++;
                    break;
                case 3:
                    stickiness++;
                    break;
            }
        }

        // Upgrade ammo
        ammo = Mathf.Min(MAX_AMMO, ammo + AMMO_UPGRADE_AMOUNT);
        currentTotalStats += numStats;
        upgradeSideEffect();
        vialColor = calculateColor();

        return true;
    }


    // Main private helper function to obtain specialization if possible
    //  Pre: only updates IFF current side effect has no specialization && one of the stats have reached side effect threshold
    //  Post: Updates the side effects to one that has specialization IFF the requirements in pre-cond holds up
    private void upgradeSideEffect() {
        // Only upgrade is the current side effect has no specialization
        if (sideEffect.getSpecialization() == Specialization.NONE) {
            // Make a possible list 
            List<Specialization> possibleSpecializations = new List<Specialization>();

            // Add to the list if certain conditions are met
            if (potency >= SIDE_EFFECT_UPGRADE_THRESHOLD) {
                possibleSpecializations.Add(Specialization.POTENCY);
            }

            if (poison >= SIDE_EFFECT_UPGRADE_THRESHOLD) {
                possibleSpecializations.Add(Specialization.POISON);
            }

            if (reactivity >= SIDE_EFFECT_UPGRADE_THRESHOLD) {
                possibleSpecializations.Add(Specialization.REACTIVITY);
            }

            if (stickiness >= SIDE_EFFECT_UPGRADE_THRESHOLD) {
                possibleSpecializations.Add(Specialization.STICKINESS);
            }

            
            // If there are stats available, choose a specialization
            if (possibleSpecializations.Count > 0) {
                Specialization selectedSpecialization = possibleSpecializations[Random.Range(0, possibleSpecializations.Count)];
                sideEffect = PoisonVialDatabase.getRandomSideEffect(selectedSpecialization);
            }

        }
    }


    // Function to upgrade Poison using only two ingredients
    //  Pre: ing1 != null && ing2 != null
    //  Post: Returns whether upgrade is successful. If successful, vial is updated with this ingredient
    public bool upgrade(Ingredient ing1, Ingredient ing2) {
        Debug.Assert(ing1 != null && ing2 != null);

        if (currentTotalStats + (2 * Ingredient.NUM_STATS_CONTRIBUTED) > MAX_TOTAL_STATS) {
            return false;
        }

        bool success1 = upgrade(ing1);
        bool success2 = upgrade(ing2);

        Debug.Assert(success1 && success2);
        return true;
    }


    // Function to access the color of this vial
    //  Pre: null
    //  Post: Returns a valid color based on stats
    public Color getColor() {
        return vialColor;
    }


    // Funtion to calculate color so that you don't have to constantly calculate color every time color is accessed
    private Color calculateColor() {
        Color maxColor;
        int maxStatValue;

        // switch statement for getting max color
        switch (sideEffect.getSpecialization()) {
            case Specialization.POTENCY:
                maxStatValue = potency;
                maxColor = potentColor;
                break;

            case Specialization.POISON:
                maxStatValue = poison;
                maxColor = poisonColor;
                break;

            case Specialization.REACTIVITY:
                maxStatValue = reactivity;
                maxColor = reactiveColor;
                break;
            
            case Specialization.STICKINESS:
                maxStatValue = stickiness;
                maxColor = stickinessColor;
                break;
            
            default:
                // Find the max stat value
                maxStatValue = potency;
                maxStatValue = Mathf.Max(maxStatValue, poison);
                maxStatValue = Mathf.Max(maxStatValue, reactivity);
                maxStatValue = Mathf.Max(maxStatValue, stickiness);

                // Get the list of colors to mix
                List<Color> maxStatColors = new List<Color>();
                if (potency == maxStatValue) {
                    maxStatColors.Add(potentColor);
                }

                if (poison == maxStatValue) {
                    maxStatColors.Add(poisonColor);
                }

                if (reactivity == maxStatValue) {
                    maxStatColors.Add(reactiveColor);
                }

                if (stickiness == maxStatValue) {
                    maxStatColors.Add(stickinessColor);
                }

                // Get max color
                maxColor = maxStatColors[0];
                for (int i = 1; i < maxStatColors.Count; i++) {
                    maxColor = (maxColor + maxStatColors[i]) / 2f;
                }
                
                break;
        }

        // Return the linear interpolation of that color
        return Color.Lerp(Color.gray, maxColor, (float)maxStatValue / (float)MAX_SINGLE_STAT);
    }


    // Main function to access total stat count
    //  Pre: none
    //  Post: returns the total number of stats. <= than maxStat
    public int getCurrentTotalStat() {
        return currentTotalStats;
    }


    // Main function to access max stat count
    //  Pre: none
    //  Post: returns max stat count for this instance
    public int getMaxTotalStat() {
        return MAX_TOTAL_STATS;
    }


    // Main function to get side effect information
    //  Pre: none
    //  Post: returns an array in the following format: [name, description], outputs the side effect's specialization as a separate object
    public string[] getSideEffectInfo(out Specialization specialization) {
        string[] sideEffectInfo = new string[2];
        sideEffectInfo[0] = sideEffect.getName();
        sideEffectInfo[1] = sideEffect.getDescription();
        
        specialization = sideEffect.getSpecialization();

        return sideEffectInfo;
    }


    // Main function to apply Enemy Aura effects
    //  Pre: aura != null, auraType is an enum within VirtualSideEffect that specifies what type of effect you're looking for, 6 >= numStacks >= 0
    //  Post: If auraType matches side effect, apply the appropriate effects
    public void applyEnemyAuraEffects(EnemyAura aura, AuraType auraType, int numStacks) {
        Debug.Assert(aura != null && numStacks >= 0 && numStacks <= 6);

        sideEffect.executeAuraDamage(aura, auraType, numStacks, this);
    }


    // If enemy aura can be present. return true;
    //  Pre: 0 <= numStacks <= 6
    //  Post: returns whether the enemy aura can be present
    public bool isEnemyAuraPresent(int numStacks) {
        Debug.Assert(numStacks >= 0 && numStacks <= 6);

        return numStacks >= AURA_THRESHOLD && sideEffect.isAuraSideEffect();
    }
}
