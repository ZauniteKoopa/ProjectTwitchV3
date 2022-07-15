using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnInEffect : IFixedEffect
{
    private float spawnInTime = 1.25f;
    private Transform owner;
    private MeshRenderer meshRender;

    // On awake, set active to false
    private void Awake() {
        meshRender = GetComponent<MeshRenderer>();
    }


    // Function used to activate a fixed visual effect that goes on a fixed path with no consideration of collision
    //  Pre: effectDuration > 0
    //  Post: activates the pathing for the effect within the game, doesn't consider collisions
    public override void activateEffect(Vector3 startPosition, Vector3 finalPosition, float effectDuration) {
        owner = transform.parent;
        transform.parent = null;
        spawnInTime = effectDuration;
        StartCoroutine(spawnSequence(spawnInTime));
    }


    // Function to enact spawn sequence
    public IEnumerator spawnSequence(float duration) {
        meshRender.enabled = true;
        yield return new WaitForSeconds(duration);

        effectFinishedEvent.Invoke();
        meshRender.enabled = false;
        transform.parent = owner;
        transform.localPosition = Vector3.zero;
        
    }
}
