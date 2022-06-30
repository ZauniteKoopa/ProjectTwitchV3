using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;

public class IngredientCrate : BreakableCrate
{
    [SerializeField]
    private IngredientLoot[] possibleLootDrops;
    [SerializeField]
    private int numLootDrops = 4;


    // When crate breaks, drop 4 ingredients
    public override void onBreak() { 
        if (numLootDrops <= 0 || possibleLootDrops.Length <= 0) {
            Debug.LogError("IngredientCrate not configured properly. Look at possibleLootDrops and numLootDrops");
        }

        for (int i = 0; i < numLootDrops; i++) {
            IngredientLoot curLoot = possibleLootDrops[Random.Range(0, possibleLootDrops.Length)];
            Object.Instantiate(curLoot, transform.position, Quaternion.identity);
        }
    }
}
