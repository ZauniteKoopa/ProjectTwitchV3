using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Abstract to illustrate an ultimate that's an external move (NOT A STEROID)
public abstract class IBattleUltimate : MonoBehaviour
{
    // Main abstract function to set ult properties
    //  Pre: ultParameters are RELIANT on the documentation of child classes because they can vary
    //  Post: ultimate properties have been set up
    public abstract void setUltimateProperties(float[] ultParameters);


    // Main function to activate sequence
    //  Pre: none
    //  Post: activates ultimate sequence
    public abstract void activateUltimate();


    // Main function to completely reset
    //  Pre: none
    //  Post: reset all effects so that everythig is back to normal
    public abstract void reset();
}
