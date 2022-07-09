using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu]
public class PoisonVialDatabase : ScriptableObject
{
    // Main variables to keep track of set up
    private static bool isSetUp = false;
    [SerializeField]
    private TextAsset sideEffectTSV;
    private const int FIRST_SIDE_EFFECT_ROW = 3;

    // Dictionary mapping specialization to List of Side effects
    private static Dictionary<Specialization, List<VirtualSideEffect>> sideEffects;


    // Main function to set this up
    public void initialize() {
        if (!isSetUp) {
            isSetUp = true;

            // Initialize the dictionary
            sideEffects = new Dictionary<Specialization, List<VirtualSideEffect>>();
            foreach (Specialization specialization in Enum.GetValues(typeof(Specialization))) {
                sideEffects.Add(specialization, new List<VirtualSideEffect>());
            }

            // Add in default none side effect
            sideEffects[Specialization.NONE].Add(new VirtualSideEffect());

            // From TSV file, Parse out rows and then get the number of rows that must be considered (found in first row)
            string[] tsvRows = sideEffectTSV.text.Split('\n');
            string[] firstRow = tsvRows[0].Split('\t');
            int numSideEffects = int.Parse(firstRow[1]);

            // Parse CSV (right now it's hardcoded)
            for (int i = FIRST_SIDE_EFFECT_ROW; i < FIRST_SIDE_EFFECT_ROW + numSideEffects; i++) {
                string[] curSideEffectRow = tsvRows[i].Split('\t');
                Specialization spec;
                VirtualSideEffect sideEffect = parseSideEffect(curSideEffectRow, i + 1, out spec);

                // If parsed side effect not successful (is null), skip the row. Else, add it to one of the lists
                if (sideEffect != null) {
                    sideEffects[spec].Add(sideEffect);
                }
            }
        }
    }


    // Main static function to get random side effect with specific specialization
    //  Pre: specialization can be any of the specializations listed in SideEffect class. isSetUp MUST be true
    //  Post: returns a side effect that's categorized with that specified specialization
    public static VirtualSideEffect getRandomSideEffect(Specialization specialization) {
        if (!isSetUp) {
            Debug.LogError("Database has not been set up yet. Accessing this function must come AFTER awake. Did you forget to put an initial loader in your level?");
        }

        // return new NoTouching();

        List<VirtualSideEffect> specializedList = sideEffects[specialization];
        return specializedList[UnityEngine.Random.Range(0, specializedList.Count)];
    }


    // Private helper function to parse out a side effect
    //  Pre: tsvRow.length >= 3, rowNumber > 0
    //  Post: If successful, returns an actual side effect to be used in database. Else, return null
    private VirtualSideEffect parseSideEffect(string[] tsvRow, int rowNumber, out Specialization spec) {
        Debug.Assert(tsvRow.Length >= 3 && rowNumber > 0);

        // Parse basic information
        string rawName = tsvRow[0];
        string rawDescription = tsvRow[1];
        spec = parseSpecialization(tsvRow[2], rowNumber);

        // If spec is NONE, return null
        if (spec == Specialization.NONE) {
            return null;
        }

        // Switch statement concerning Name
        switch (rawName) {
            case "Spray and Pray":
                return new SprayAndPray(rawDescription, spec, float.Parse(tsvRow[4]), float.Parse(tsvRow[6]));
            case "Contagion":
                return new Contagion(rawDescription, spec, float.Parse(tsvRow[4]));
            case "Faster Decay":
                return new FasterDecay(rawDescription, spec, float.Parse(tsvRow[4]));
            case "Radioactive Expunge":
                return new RadioactiveExpunge(rawDescription, spec, float.Parse(tsvRow[4]));
            case "Induced Paralysis":
                return new InducedParalysis(rawDescription, spec, float.Parse(tsvRow[4]));
            default:
                Debug.LogWarning("Warning: In row " + rowNumber + " name given was not classified. Is this a typo: " + rawName);
                return null;
        }
    }


    // Private helper function to parse specialization from a raw string
    //  Pre: specialization can be any string, rowNumber > 0
    //  Post: returns the specialization associated with the string. If none found, return NONE;
    private Specialization parseSpecialization(string specialization, int rowNumber) {
        Debug.Assert(rowNumber > 0);

        specialization = specialization.ToLower();

        switch (specialization) {
            case "potency":
                return Specialization.POTENCY;
            case "poison":
                return Specialization.POISON;
            case "reactivity":
                return Specialization.REACTIVITY;
            case "stickiness":
                return Specialization.STICKINESS;
            default:
                Debug.LogWarning("Warning: In Row " + rowNumber +" specialization is misspelled: " + specialization);
                return Specialization.NONE;
        }
    }
}
