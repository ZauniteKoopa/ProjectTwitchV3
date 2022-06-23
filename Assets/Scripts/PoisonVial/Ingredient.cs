using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

public class Ingredient
{
    // Private instance variables
    private string name;
    private float[] statProbabilities;

    public static readonly int NUM_STATS_CONTRIBUTED = 2;
    private const int NUM_STATS_TOTAL = 4;

    private const int POTENCY_INDEX = 0;
    private const int POISON_INDEX = 1;
    private const int REACTIVITY_INDEX = 2;
    private const int STICKINESS_INDEX = 3;

    private Color ingredientColor = Color.black;


    // Main constructor for an ingredient
    //  Pre: all stat chances added must be greater than 0f
    //  Post: Ingredient object has been constructed
    public Ingredient(float pot, float poi, float r, float s, string n, Color color) {
        Debug.Assert(pot >= 0f && poi >= 0f && r >= 0f && s >= 0f);

        // Get total to normalize probability
        statProbabilities = new float[NUM_STATS_TOTAL];
        float total = pot + poi + r + s;

        // Normalize probability so that all of the chances sum up to 1.0f
        statProbabilities[POTENCY_INDEX] = pot / total;
        statProbabilities[POISON_INDEX] = poi / total;
        statProbabilities[REACTIVITY_INDEX] = r / total;
        statProbabilities[STICKINESS_INDEX] = s / total;

        // Set name
        name = n;
        ingredientColor = color;
    }


    // Main function to get modified stat probabilities if one stat is unable to upgrade
    private float[] getModifiedProbabilities(int ignoredIndex) {
        float[] modifiedProb = new float[NUM_STATS_TOTAL];
        float totalProbability = 0.0f;

        // Do one pass to get total probability
        for (int i = 0; i < NUM_STATS_TOTAL; i++) {
            if (i != ignoredIndex) {
                totalProbability += statProbabilities[i];
            }
        }

        // Do another pass to renormalize probabilities without ignoredIndex
        for (int i = 0; i < NUM_STATS_TOTAL; i++) {
            modifiedProb[i] = (i == ignoredIndex) ? 0.0f : statProbabilities[i] / totalProbability;
        }

        return modifiedProb;
    }


    // Main function to get stat gain
    //  Pre: ignoredStatIndex is the stat to ignore, refering to this order: [Potency, Poison, Reactivity, Stickiness]. If negative, ignore no stat
    //  Post: returns an int representing the stat in the index order described in pre-cond
    public int calculateStatGain(int ignoredStatIndex = -1) {
        // Get the stat probabilities to be used
        float[] usedProbabilities = (ignoredStatIndex < 0) ? statProbabilities : getModifiedProbabilities(ignoredStatIndex);

        // Calculate which stat using rng
        float diceRoll = Random.Range(0f, 1f);
        int currentStat = 0;

        // Go down the list of probabilities to see if you get current stat
        while (currentStat < usedProbabilities.Length && diceRoll > usedProbabilities[currentStat]) {
            diceRoll -= usedProbabilities[currentStat];
            currentStat++;
        }

        Debug.Assert(currentStat < usedProbabilities.Length);
        return currentStat;
    }


    // Main function to present stat chances
    //  Pre: probDisplays.Length == 4 and match in this order [Potency, Poison, Reactivity, Stickiness]
    //  Post: update probability displays to show the probability chance of getting stat contributed
    public void displayStatChances(TMP_Text[] probDisplays) {
        Debug.Assert(probDisplays.Length == NUM_STATS_TOTAL);

        for (int i = 0; i < NUM_STATS_TOTAL; i++) {
            int percentChance = Mathf.RoundToInt(statProbabilities[i] * 100f);
            probDisplays[i].text = percentChance + "%";
        }
    }


    // Main function to get name
    public string getName() {
        return name;
    }


    // Main function to get the hashcode of a specific object: mostly tied to the name
    public override int GetHashCode() {
        return name.GetHashCode();
    }


    // Main function to convert this ingredient to a string
    public string toString() {
        return name + ": Potency-" + statProbabilities[POTENCY_INDEX] + ", Poison-" + statProbabilities[POISON_INDEX] + ", Reactivity-" + statProbabilities[REACTIVITY_INDEX] + ", Stickiness-" + statProbabilities[STICKINESS_INDEX];
    }


    // Main function to get the color of this ingredient
    public Color getColor() {
        return ingredientColor;
    }

    
    // Main function to access the number of stats contributed by this ingredient
    public int getNumStatContribution() {
        return NUM_STATS_CONTRIBUTED;
    }
}
