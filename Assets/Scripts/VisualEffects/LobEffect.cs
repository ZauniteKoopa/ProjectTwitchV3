using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobEffect : IFixedEffect
{
    // Private instance variables
    private bool activated = false;
    private const float LOB_HEIGHT = 3.5f;


    // Public function to activate effect, can only be activated once
    //  Pre: effectDuration > 0
    //  Post: Starts the lob sequence if this hasn't been started already
    public override void activateEffect(Vector3 startPosition, Vector3 finalPosition, float effectDuration) {
        if (!activated) {
            activated = true;
            StartCoroutine(lobEffectSequence(startPosition, finalPosition, effectDuration));
        }
    }

    
    // Lob sequence to lob the current gameobject into the air
    //  Pre: startPosition.y == finalPosition.y and effectDuration > 0
    //  Post: transform will go to a lobbing motion in the air
    private IEnumerator lobEffectSequence(Vector3 startPos, Vector3 finalPos, float effectDuration) {

        // Calculate middle position
        float middleHeight = Mathf.Max(startPos.y, finalPos.y) + LOB_HEIGHT;
        Vector3 middlePos = ((startPos + finalPos) / 2.0f);
        middlePos = new Vector3(middlePos.x, middleHeight, middlePos.z);

        // Create variables for timers
        float timer = 0.0f;
        float slopedDuration = effectDuration / 2.0f;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        // Ascending lob
        while(timer <= slopedDuration) {
            yield return waitFrame;

            timer += Time.fixedDeltaTime;
            transform.position = Vector3.Slerp(startPos, middlePos, timer / slopedDuration);
        }

        // Descending lob
        timer -= slopedDuration;

        while(timer <= slopedDuration) {
            yield return waitFrame;

            timer += Time.fixedDeltaTime;
            transform.position = Vector3.Slerp(middlePos, finalPos, timer / slopedDuration);
        }

        // Destroy the object at the end of this coroutine
        Object.Destroy(gameObject);
    }
}
