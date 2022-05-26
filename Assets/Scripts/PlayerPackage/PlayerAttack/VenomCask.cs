using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VenomCask : MonoBehaviour
{
    // Private instance variables
    [Header("Reference Variables")]
    [SerializeField]
    private CaskZone poisonFogHitbox;
    [SerializeField]
    private IFixedEffect caskThrowVisualEffect;

    // Variables concerning PoisonCask sequence
    [SerializeField]
    private float throwDuration = 0.5f;
    [SerializeField]
    private float initialDamageDuration = 0.08f;
    [SerializeField]
    private int numTicks = 4;
    [SerializeField]
    private float timePerTick = 0.55f;
    private bool launched = false;

    
    // On Awake, deactivate poisonFogHitbox
    private void Awake() {
        // Error check
        if (poisonFogHitbox == null) {
            Debug.LogError("Fog hitbox not connected to cask object for " + transform, transform);
        } else if (caskThrowVisualEffect == null) {
            Debug.LogError("Throw visual effect not connected to cask object for " + transform, transform);
        }

        // Set state of objects
        poisonFogHitbox.gameObject.SetActive(false);
        caskThrowVisualEffect.gameObject.SetActive(true);
    }


    // Public method to launch cask at set final destination
    public void launch(Vector3 dest, IVial poison) {
        if (!launched) {
            launched = true;
            poisonFogHitbox.setCaskPoison(poison);
            StartCoroutine(throwVenomCaskSequence(dest, poison));
        }
    }

    
    // Private coroutine to do the throw sequence
    //  Pre: finalDestination is the set destination that cask will be thrown at. This will not dynamically change
    private IEnumerator throwVenomCaskSequence(Vector3 finalDestination, IVial poison) {
        caskThrowVisualEffect.activateEffect(transform.position, finalDestination, throwDuration);
        yield return new WaitForSeconds(throwDuration);

        // Activate poison fog hitbox and do some immediate damage
        poisonFogHitbox.transform.position = finalDestination;
        poisonFogHitbox.gameObject.SetActive(true);
        yield return new WaitForSeconds(initialDamageDuration);

        poisonFogHitbox.damageAllTargets(poison.getInitCaskDamage());


        for (int i = 0; i < numTicks; i++) {
            yield return new WaitForSeconds(timePerTick);

            // Do tick damage and increase stack to all enemies found in hitbox
            poisonFogHitbox.damageAllTargets(0.0f);
        }

        Object.Destroy(gameObject);
    }

}
