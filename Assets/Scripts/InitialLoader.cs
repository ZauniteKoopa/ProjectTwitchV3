using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialLoader : MonoBehaviour
{
    // Main component to initialize elements first
    [SerializeField]
    private IngredientDatabase ingredientDatabase;
    [SerializeField]
    private PoisonVialDatabase vialDatabase;

    // Start is called before the first frame update
    void Awake()
    {
        if (ingredientDatabase == null) {
            Debug.LogError("Loader not connected to ingredient database! Ingredient information will not load, leading to errors");
        }

        if (vialDatabase == null) {
            Debug.LogError("Loader not connected to vial database! Poison vial information will not load, leading to errors");
        }

        ingredientDatabase.parseCSV();
        vialDatabase.initialize();
        PoisonVial.parseBaseVialCSV();
    }
}
