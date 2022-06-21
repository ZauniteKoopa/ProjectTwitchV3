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
    private const int MAX_AMMO = 60;
    private const int ONE_ING_AMMO = 40;
    private const int TWO_ING_AMMO = 60;
    private const int AMMO_UPGRADE_AMOUNT = 10;

    // Colors for color mixing
    private static Color potentColor = Color.red;
    private static Color poisonColor = Color.green;
    private static Color reactiveColor = new Color(0.5f, 0f, 1f);
    private static Color stickinessColor = Color.blue;
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



    // Main raw constructor for a poison vial
    //  Pre: initialAmmo > 0 and 0 <= all stats <= 5
    public PoisonVial(int pot, int poi, int r, int s, int initialAmmo) {
        Debug.Assert(initialAmmo > 0);
        Debug.Assert(pot >= 0 && pot <= 5);
        Debug.Assert(poi >= 0 && poi <= 5);
        Debug.Assert(r >= 0 && r <= 5);
        Debug.Assert(s >= 0 && s <= 5);

        if (!csvParsed) {
            parseBaseVialCSV();
        }

        potency = pot;
        poison = poi;
        reactivity = r;
        stickiness = s;
        currentTotalStats = pot + poi + r + s;
        vialColor = calculateColor();

        ammo = initialAmmo;
        sideEffect = new VirtualSideEffect();
    }


    // Main constructor to craft a poison vial from only 1 ingredient
    public PoisonVial(Ingredient ing) {
        if (!csvParsed) {
            parseBaseVialCSV();
        }

        potency = 0;
        poison = 0;
        reactivity = 0;
        stickiness = 0;
        currentTotalStats = 0;

        sideEffect = new VirtualSideEffect();
        upgrade(ing);
        ammo = ONE_ING_AMMO;
    }


    // Main constructor to craft a poison vial from only 1 ingredient
    public PoisonVial(Ingredient ing1, Ingredient ing2) {
        if (!csvParsed) {
            parseBaseVialCSV();
        }

        potency = 0;
        poison = 0;
        reactivity = 0;
        stickiness = 0;
        currentTotalStats = 0;

        sideEffect = new VirtualSideEffect();
        upgrade(ing1, ing2);
        ammo = TWO_ING_AMMO;
    }


    // Private helper function to parse the CSV
    //  Pre: csvParsed = false
    //  Post: static variables have been updated to reflect csv values
    private void parseBaseVialCSV() {
        Debug.Assert(!csvParsed);

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

        csvParsed = true;
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
    public int getMaxVialSize() {
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

        // Check if there's any stats to ignore when upgrading (reached single max)
        int ignoredIndex = -1;
        ignoredIndex = (potency >= MAX_SINGLE_STAT) ? 0 : ignoredIndex;
        ignoredIndex = (poison >= MAX_SINGLE_STAT) ? 1 : ignoredIndex;
        ignoredIndex = (reactivity >= MAX_SINGLE_STAT) ? 2 : ignoredIndex;
        ignoredIndex = (stickiness >= MAX_SINGLE_STAT) ? 3 : ignoredIndex;

        // Calculate stat gains: make sure you either contribute all stats or just enough to hit 
        int numStats = Mathf.Min(Ingredient.NUM_STATS_CONTRIBUTED, MAX_TOTAL_STATS - currentTotalStats);
        Dictionary<string, int> statGains = ing.calculateStatGains(numStats, ignoredIndex);

        // Upgrade stats
        potency += statGains["Potency"];
        poison += statGains["Poison"];
        reactivity += statGains["Reactivity"];
        stickiness += statGains["Stickiness"];
        currentTotalStats += numStats;

        // Upgrade ammo
        ammo = Mathf.Min(MAX_AMMO, ammo + AMMO_UPGRADE_AMOUNT);
        upgradeSideEffect();
        vialColor = calculateColor();

        return true;
    }


    // Main private helper function to obtain specialization if possible
    //  Pre: only updates IFF current side effect has no specialization && one of the stats have reached side effect threshold
    //  Post: Updates the side effects to one that has specialization IFF the requirements in pre-cond holds up
    private void upgradeSideEffect() {
        if (sideEffect.getSpecialization() == Specialization.NONE) {
            sideEffect = new SprayAndPray();
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
        // Get interpolated colors with a magnitude of 1/10 the color spectrum distance (white to black)
        Color normPotency = Color.Lerp(Color.black, potentColor, 1f / (float)currentTotalStats);
        Color normPoison = Color.Lerp(Color.black, poisonColor, 1f / (float)currentTotalStats);
        Color normReactivity = Color.Lerp(Color.black, reactiveColor, 1f / (float)currentTotalStats);
        Color normStickiness = Color.Lerp(Color.black, stickinessColor, 1f / (float)currentTotalStats);

        // Mix colors via vector addition
        Vector3 finalColorVector = Vector3.zero;
        finalColorVector += (new Vector3(normPotency.r, normPotency.g, normPotency.b) * potency);
        finalColorVector += (new Vector3(normPoison.r, normPoison.g, normPoison.b) * poison);
        finalColorVector += (new Vector3(normReactivity.r, normReactivity.g, normReactivity.b) * reactivity);
        finalColorVector += (new Vector3(normStickiness.r, normStickiness.g, normStickiness.b) * stickiness);

        // Get color components individually
        float redComp = Mathf.Min(finalColorVector.x, 1.0f);
        float greenComp = Mathf.Min(finalColorVector.y, 1.0f);
        float blueComp = Mathf.Min(finalColorVector.z, 1.0f);

        return new Color(redComp, greenComp, blueComp);
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
    //  Post: returns an array in the following format: [name, description]
    public string[] getSideEffectInfo() {
        string[] sideEffectInfo = new string[2];
        sideEffectInfo[0] = sideEffect.getName();
        sideEffectInfo[1] = sideEffect.getDescription();

        return sideEffectInfo;
    }
}
