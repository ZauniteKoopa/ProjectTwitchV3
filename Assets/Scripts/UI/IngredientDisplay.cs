using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientDisplay : MonoBehaviour
{
    // UI Elements: Prob Displays are in the following order: Potency, Poison, Reactivity, Stickiness
    [SerializeField]
    private TMP_Text[] probabilityDisplays;
    [SerializeField]
    private Image ingredientImage;


    // Public function to display information
    public void displayIngredient(Ingredient ing) {
        if (ing != null) {
            ing.displayStatChances(probabilityDisplays);
            ingredientImage.color = ing.getColor();
        } else {
            clear();
        }
    }


    // Main function to clear ingredient display: display will just not be shown
    public void clear() {
        gameObject.SetActive(false);
    }

    
    // Main function to clear without disabling
    public void uiClear() {
        foreach (TMP_Text probDisplay in probabilityDisplays) {
            probDisplay.text = "0%";
        }

        ingredientImage.color = Color.black;
    }
}
