using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextNumberDisplay : INumberDisplay
{
    [SerializeField]
    private TMP_Text label = null;


    // Main function to change the number displayed
    public override void displayNumber(int number) {
        label.text = number + "";
    }

}
