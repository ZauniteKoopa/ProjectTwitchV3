using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbingUltimate : MonoBehaviour
{
    [SerializeField]
    private IFixedEffect lobVFX;
    [SerializeField]
    private IBattleUltimate ultimateHitbox;

    // Variables concerning throw
    [SerializeField]
    private float throwDuration = 0.5f;
    private bool launched = false;


    // On first frame, error check and disable ultimate
    private void Awake() {
        // Error check
        if (ultimateHitbox == null) {
            Debug.LogError("Ultimate hitbox not connected to cask object for " + transform, transform);
        } else if (lobVFX == null) {
            Debug.LogError("Throw visual effect not connected to cask object for " + transform, transform);
        }

        // Set state of objects
        ultimateHitbox.gameObject.SetActive(false);
        lobVFX.gameObject.SetActive(true);
    }


    // Public method to launch cask at set final destination
    public void launch(Vector3 dest, float[] ultParameters) {
        if (!launched) {
            launched = true;
            ultimateHitbox.setUltimateProperties(ultParameters);
            StartCoroutine(throwUltimateSequence(dest));
        }
    }

    
    // Private coroutine to do the throw sequence
    //  Pre: finalDestination is the set destination that cask will be thrown at. This will not dynamically change
    private IEnumerator throwUltimateSequence(Vector3 finalDestination) {
        lobVFX.activateEffect(transform.position, finalDestination, throwDuration);
        yield return new WaitForSeconds(throwDuration);

        // Activate ultimate
        ultimateHitbox.transform.position = finalDestination;
        ultimateHitbox.transform.parent = null;
        ultimateHitbox.gameObject.SetActive(true);
        ultimateHitbox.activateUltimate();
        
        // Destroy parent object
        Object.Destroy(gameObject);
    }
}
