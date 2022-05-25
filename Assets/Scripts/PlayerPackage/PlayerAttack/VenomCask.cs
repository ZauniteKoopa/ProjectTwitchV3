using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VenomCask : MonoBehaviour
{
    // Private instance variables
    [Header("Reference Variables")]
    [SerializeField]
    private GameObject poisonFogHitbox;
    [SerializeField]
    private IFixedEffect caskThrowVisualEffect;

    // Variables concerning PoisonCask sequence
    [SerializeField]
    private float throwDuration = 0.5f;
    [SerializeField]
    private int numTicks = 4;
    [SerializeField]
    private float timePerTick = 0.55f;
    private bool launched = false;

    
    // On Awake, deactivate poisonFogHitbox
    private void Awake() {
        poisonFogHitbox.SetActive(false);
        caskThrowVisualEffect.gameObject.SetActive(true);
    }


    // Public method to launch cask at set final destination
    public void launch(Vector3 dest) {
        if (!launched) {
            launched = true;
            StartCoroutine(throwVenomCaskSequence(dest));
        }
    }

    
    // Private coroutine to do the throw sequence
    //  Pre: finalDestination is the set destination that cask will be thrown at. This will not dynamically change
    private IEnumerator throwVenomCaskSequence(Vector3 finalDestination) {
        caskThrowVisualEffect.activateEffect(transform.position, finalDestination, throwDuration);
        yield return new WaitForSeconds(throwDuration);

        poisonFogHitbox.transform.position = finalDestination;
        poisonFogHitbox.SetActive(true);

        for (int i = 0; i < numTicks; i++) {
            yield return new WaitForSeconds(timePerTick);

            // Do tick damage and increase stack to all enemies found in hitbox
            Debug.Log("doTickDamage");
        }

        Object.Destroy(gameObject);
    }

}
