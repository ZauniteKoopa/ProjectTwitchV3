using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnInEffect : IFixedEffect
{
    private float spawnInTime = 1.25f;

    // On awake, set active to false
    private void Awake() {
        StartCoroutine(spawnSequence(spawnInTime));
    }


    // Function used to activate a fixed visual effect that goes on a fixed path with no consideration of collision
    //  Pre: effectDuration > 0
    //  Post: activates the pathing for the effect within the game, doesn't consider collisions
    public override void activateEffect(Vector3 startPosition, Vector3 finalPosition, float effectDuration) {
        transform.parent = null;
        spawnInTime = effectDuration;
        gameObject.SetActive(true);
    }


    // Function to enact spawn sequence
    public IEnumerator spawnSequence(float duration) {
        yield return new WaitForSeconds(duration);

        effectFinishedEvent.Invoke();
        Object.Destroy(gameObject);
    }
}
