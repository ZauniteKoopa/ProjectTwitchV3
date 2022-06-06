using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialLoader : MonoBehaviour
{
    // Main component to initialize elements first
    [SerializeField]
    private IngredientDatabase ingredientDatabase;

    // Start is called before the first frame update
    void Awake()
    {
        ingredientDatabase.parseCSV();
    }
}
