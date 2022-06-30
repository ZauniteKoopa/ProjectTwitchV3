using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableCrate : IHittable
{
    // Main function to hit the object with an attack
    //  Pre: none
    //  Post: activates an object on hit
    public override void hit() {
        gameObject.SetActive(false);
        onBreak();
    }


    // Main function to reset IHittable
    //  Pre: none
    //  Post: resets anything that was hit
    public override void reset() {
        gameObject.SetActive(true);
    }


    // Main function to handle when it breaks
    public virtual void onBreak() {}
}
