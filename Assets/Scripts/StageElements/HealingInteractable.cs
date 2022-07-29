using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingInteractable : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 1f)]
    private float healthPercent = 0.5f;

    [SerializeField]
    private float healingTime = 1.0f;


    // On trigger enter
    private void OnTriggerEnter(Collider collider) {
        ITwitchStatus testPlayer = collider.GetComponent<ITwitchStatus>();

        if (testPlayer != null) {
            testPlayer.applyHealthRegenEffect(healthPercent, healingTime);
            Object.Destroy(gameObject);
        }
    }
}
