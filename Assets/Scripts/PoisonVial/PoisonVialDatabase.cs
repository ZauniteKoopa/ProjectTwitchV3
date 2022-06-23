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

            // Parse CSV (right now it's hardcoded)
            VirtualSideEffect curSideEffect = new VirtualSideEffect();
            sideEffects[Specialization.NONE].Add(curSideEffect);

            curSideEffect = new SprayAndPray();
            sideEffects[Specialization.POTENCY].Add(curSideEffect);

            curSideEffect = new FasterDecay();
            sideEffects[Specialization.POISON].Add(curSideEffect);

            curSideEffect = new Contagion();
            sideEffects[Specialization.POISON].Add(curSideEffect);

            curSideEffect = new RadioactiveExpunge();
            sideEffects[Specialization.REACTIVITY].Add(curSideEffect);

            curSideEffect = new InducedParalysis();
            sideEffects[Specialization.STICKINESS].Add(curSideEffect);
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
