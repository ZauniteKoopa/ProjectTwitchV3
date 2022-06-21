using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextNumberDisplay : INumberDisplay
{
    [SerializeField]
    private TMP_Text label;


    // Main function to change the number displayed
    public override void displayNumber(int number) {
        label.text = number + "";
    }

    // Main function to change the color of this INumberDisplay
    //  Pre: color is the color you want to change to
    //  Post: updates color accordingly
    public override void displayColor(Color color) {
        label.color = color;
    }
}
