using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IUnitStatus : MonoBehaviour
{
    // Main method to get current movement speed considering all speed status effects on unit
    //  Pre: none
    //  Post: Returns movement speed with speed status effects in mind
    public abstract float getMovementSpeed();
}
