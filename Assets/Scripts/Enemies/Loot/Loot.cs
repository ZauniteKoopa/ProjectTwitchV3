using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Loot : MonoBehaviour
{
    [SerializeField]
    private IFixedEffect lootDropVisualEffect;
    [SerializeField]
    private float dropDuration = 0.45f;
    [SerializeField]
    private LayerMask lootLayerMask;
    [SerializeField]
    private float maxLootDistance = 2.5f;
    [SerializeField]
    private float minLootDistance = 0.25f;
    [SerializeField]
    private float lootThrowWallOffset = 0.5f;
    private float lootGroundOffset = 0.25f;

    // Reference variables
    private MeshRenderer meshRender;
    private Collider lootTrigger;

    // On Awake, deactivate poisonFogHitbox
    private void Awake() {
        // Error check
        if (lootDropVisualEffect == null) {
            Debug.LogError("Throw visual effect not connected to loot object for " + transform, transform);
        }

        // Get reference variables
        meshRender = GetComponent<MeshRenderer>();
        lootTrigger = GetComponent<Collider>();

        // Set status variables
        lootDropVisualEffect.gameObject.SetActive(true);
        lootTrigger.enabled = false;
        meshRender.enabled = false;
        StartCoroutine(lootDropSequence());
    }

    // Loot drop sequence
    private IEnumerator lootDropSequence() {
        // Wait a frame just in case of radius changes
        yield return new WaitForFixedUpdate();

        // Get random ray properties for raycast
        Vector3 rayDir = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
        rayDir = rayDir.normalized;
        float rayDistance = Random.Range(minLootDistance, maxLootDistance);

        // Shoot out raycast and check for collision
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(transform.position, rayDir, out hitInfo, rayDistance, lootLayerMask);
        Vector3 finalDestination = transform.position + (rayDistance * rayDir);

        if (hit) {
            finalDestination = hitInfo.point - (lootThrowWallOffset * rayDir);
        }

        // Get ground height
        bool groundHit = Physics.Raycast(finalDestination, Vector3.down, out hitInfo, Mathf.Infinity, lootLayerMask);
        if (groundHit) {
            finalDestination.y = (hitInfo.point.y + lootGroundOffset);
        }

        // Launch visual effect and wait for it to end
        lootDropVisualEffect.activateEffect(transform.position, finalDestination, dropDuration);
        yield return new WaitForSeconds(dropDuration);

        // Activate gameobject and move to final position
        transform.position = finalDestination;
        meshRender.enabled = true;
        yield return new WaitForSeconds(0.25f);
        lootTrigger.enabled = true;
    }


    // Main function to interact with ingredient
    public virtual bool onPlayerCollect (ITwitchInventory inventory) {
        Object.Destroy(gameObject);
        return true;
    }


    // Main function to quick craft with player 
    public virtual bool onPlayerQuickCraft(ITwitchInventory inventory, bool isPrimary) {
        Object.Destroy(gameObject);
        return true;
    }

    // Main function to get ingredient associated with this loot
    public virtual Ingredient getIngredient() {
        return null;
    }

    // Main function to set distances
    //  Pre: maxSpawnRadius >= minSpawnRadius >= 0
    //  Post: spawn radiuses are set
    public virtual void setSpawnRadius(float minSpawnRadius, float maxSpawnRadius) {
        Debug.Assert(minSpawnRadius >= 0f && maxSpawnRadius >= minSpawnRadius);

        minLootDistance = minSpawnRadius;
        maxLootDistance = maxSpawnRadius;
    }
}
