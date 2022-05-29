using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class INumberDisplay : MonoBehaviour
{
    // Main function to display a positive number, whether through icons or text
    //  Pre: number is the value you want to display
    //  Post: displays value in whatever way the display wants
    public abstract void displayNumber(int number);
    
}
