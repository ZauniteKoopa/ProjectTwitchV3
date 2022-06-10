using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class IngredientDatabase : ScriptableObject
{
    // Instance variables
    [SerializeField]
    private TextAsset ingredientCSV;
    private static bool parsedCSVs = false;
    private static Dictionary<string, Ingredient> database;

    private const int STARTING_PARSE_ROW = 2;


    // Public function to parse CSV (MUST BE DONE BEFORE ASKING FOR INGREDIENTS)
    public void parseCSV() {
        if (ingredientCSV == null) {
            Debug.LogError("No data attached to this database in the form of a CSV file");
        }

        // If you have not parsedCSVs yet, parse them
        if (!parsedCSVs) {
            // Initialize database
            database = new Dictionary<string, Ingredient>();
            parsedCSVs = true;

            // Parse out rows and then get the number of rows that must be considered (found in first row)
            string[] csvRows = ingredientCSV.text.Split('\n');
            string[] firstRow = csvRows[0].Split(',');
            int numIngredients = int.Parse(firstRow[1]);

            // Go through each row
            for (int r = STARTING_PARSE_ROW; r < STARTING_PARSE_ROW + numIngredients; r++) {
                string[] currentRow = csvRows[r].Split(',');

                // Parse data: ASSUMES PERCENTAGE FORMAT
                string ingName = currentRow[0];
                float potencyChance = float.Parse(currentRow[1].Substring(0, currentRow[1].Length - 1));
                float poisonChance = float.Parse(currentRow[2].Substring(0, currentRow[2].Length - 1));
                float reactivityChance = float.Parse(currentRow[3].Substring(0, currentRow[3].Length - 1));
                float stickinessChance = float.Parse(currentRow[4].Substring(0, currentRow[4].Length - 1));
                string hexColor = currentRow[5];

                // Parse color
                Color ingColor;
                if (!ColorUtility.TryParseHtmlString(hexColor, out ingColor)) {
                    Debug.LogError("Error in row " + (r + 1) + ": color not in HTML hexadecimal format");
                }

                // Make ingredient
                Ingredient currentIng = new Ingredient(potencyChance, poisonChance, reactivityChance, stickinessChance, ingName, ingColor);
                database.Add(ingName, currentIng);
            }
        }
    }


    // Main function to get Ingredient referenced by a specific name seen within the database
    //  Pre: returns null if not found
    public static Ingredient getIngredient(string name) {
        return (database.ContainsKey(name)) ? database[name] : null;
    }
}
