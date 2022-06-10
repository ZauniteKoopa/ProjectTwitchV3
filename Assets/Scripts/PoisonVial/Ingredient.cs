using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


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


    // Main function to craft an ingredient
    //  Pre: none
    //  Post: returns a dictionary that maps Poison Vial stats to num of stat gains
    public Dictionary<string, int> calculateStatGains() {
        // Initialize dictionary
        Dictionary<string, int> statGains = new Dictionary<string, int>();
        statGains.Add("Potency", 0);
        statGains.Add("Poison", 0);
        statGains.Add("Reactivity", 0);
        statGains.Add("Stickiness", 0);

        // Add to stat buffs by rolling the dice twice
        for (int i = 0; i < NUM_STATS_CONTRIBUTED; i++) {
            float diceRoll = Random.Range(0f, 1f);
            int currentStat = 0;

            // Go down the list of probabilities to see if you get current stat
            while (currentStat < statProbabilities.Length && diceRoll > statProbabilities[currentStat]) {
                diceRoll -= statProbabilities[currentStat];
                currentStat++;
            }

            Debug.Assert(currentStat < statProbabilities.Length);

            // Increment dictionary
            statGains["Potency"] += (currentStat == POTENCY_INDEX) ? 1 : 0;
            statGains["Poison"] += (currentStat == POISON_INDEX) ? 1 : 0;
            statGains["Reactivity"] += (currentStat == REACTIVITY_INDEX) ? 1 : 0;
            statGains["Stickiness"] += (currentStat == STICKINESS_INDEX) ? 1 : 0;
        }

        // Return dictionary
        Debug.Assert(statGains.ContainsKey("Potency") && statGains.ContainsKey("Poison") && statGains.ContainsKey("Reactivity") && statGains.ContainsKey("Stickiness"));
        Debug.Assert(statGains["Potency"] + statGains["Poison"] + statGains["Reactivity"] + statGains["Stickiness"] == NUM_STATS_CONTRIBUTED);
        return statGains;
    }


    // Main function to present stat chances
    //  Pre: probDisplays.Length == 4 and match in this order [Potency, Poison, Reactivity, Stickiness]
    //  Post: update probability displays to show the probability chance of getting stat contributed
    public void displayStatChances(INumberDisplay[] probDisplays) {
        Debug.Assert(probDisplays.Length == NUM_STATS_TOTAL);

        for (int i = 0; i < NUM_STATS_TOTAL; i++) {
            int percentChance = Mathf.RoundToInt(statProbabilities[i] * 100f);
            probDisplays[i].displayNumber(percentChance);
        }
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
}
