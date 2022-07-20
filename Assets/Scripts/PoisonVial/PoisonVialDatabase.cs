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

    // Dictionary mapping specialization to List of Side effects
    [SerializeField]
    private List<VirtualSideEffect> baseSideEffects;
    [SerializeField]
    private List<VirtualSideEffect> potencySideEffects;
    [SerializeField]
    private List<VirtualSideEffect> poisonSideEffects;
    [SerializeField]
    private List<VirtualSideEffect> reactiveSideEffects;
    [SerializeField]
    private List<VirtualSideEffect> stickinessSideEffects;
    
    private static Dictionary<Specialization, List<VirtualSideEffect>> sideEffects;


    // Main function to set this up
    public void initialize() {
        if (!isSetUp) {
            isSetUp = true;

            // Error check
            if (baseSideEffects.Count == 0) {
                Debug.LogError("There should be 1 default / base side effect within the database");
            }

            if (potencySideEffects.Count == 0) {
                Debug.LogError("There should be at least 1 potency side effect within the database");
            }

            if (poisonSideEffects.Count == 0) {
                Debug.LogError("There should be at least 1 poison side effect within the database");
            }

            if (reactiveSideEffects.Count == 0) {
                Debug.LogError("There should be at least 1 reactive side effect within the database");
            }

            if (stickinessSideEffects.Count == 0) {
                Debug.LogError("There should be at least 1 stickiness side effect within the database");
            }

            // Set up the dictionary
            sideEffects = new Dictionary<Specialization, List<VirtualSideEffect>>();
            sideEffects.Add(Specialization.NONE, baseSideEffects);
            sideEffects.Add(Specialization.POTENCY, potencySideEffects);
            sideEffects.Add(Specialization.POISON, poisonSideEffects);
            sideEffects.Add(Specialization.REACTIVITY, reactiveSideEffects);
            sideEffects.Add(Specialization.STICKINESS, stickinessSideEffects);
        }
    }


    // Main static function to get random side effect with specific specialization
    //  Pre: specialization can be any of the specializations listed in SideEffect class. isSetUp MUST be true
    //  Post: returns a side effect that's categorized with that specified specialization
    public static VirtualSideEffect getRandomSideEffect(Specialization specialization) {
        if (!isSetUp) {
            Debug.LogError("Database has not been set up yet. Accessing this function must come AFTER awake. Did you forget to put an initial loader in your level?");
        }

        List<VirtualSideEffect> specializedList = sideEffects[specialization];
        return specializedList[UnityEngine.Random.Range(0, specializedList.Count)];
    }
}
