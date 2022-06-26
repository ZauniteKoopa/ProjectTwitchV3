using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class IHittable : MonoBehaviour
{
    // Main function to hit the object with an attack
    //  Pre: none
    //  Post: activates an object on hit
    public abstract void hit();


    // Main function to reset IHittable
    //  Pre: none
    //  Post: resets anything that was hit
    public abstract void reset();
}
