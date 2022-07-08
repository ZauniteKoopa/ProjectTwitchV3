using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class IFixedEffect : MonoBehaviour
{
    // Main event handler for when the effect is over
    public UnityEvent effectFinishedEvent;

    // Function used to activate a fixed visual effect that goes on a fixed path with no consideration of collision
    //  Pre: effectDuration > 0
    //  Post: activates the pathing for the effect within the game, doesn't consider collisions
    public abstract void activateEffect(Vector3 startPosition, Vector3 finalPosition, float effectDuration);
}
