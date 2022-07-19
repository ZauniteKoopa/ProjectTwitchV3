using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LobbedEnemy : MonoBehaviour
{
    // Reference variables
    [SerializeField]
    private IFixedEffect lobVisualEffect;
    [SerializeField]
    private Transform enemyUnit;
    [SerializeField]
    private LayerMask lobLayerMask;
    [SerializeField]
    private Transform indicator;
    [SerializeField]
    private float dropDuration = 1.5f;

    [SerializeField]
    private float throwWallOffset = 0.5f;
    private float groundOffset = 0.5f;


    private float maxSpawnRadius = 10f;
    private float minSpawnRadius = 6f;


    // On Awake, deactivate poisonFogHitbox
    private void Awake() {
        // Error check
        if (lobVisualEffect == null) {
            Debug.LogError("Throw visual effect not connected to lob enemy object for " + transform, transform);
        }

        // Set status variables
        lobVisualEffect.gameObject.SetActive(true);
        indicator.gameObject.SetActive(false);
        StartCoroutine(enemyDropSequence());
    }

    // Loot drop sequence
    private IEnumerator enemyDropSequence() {
        // Wait a frame just in case of radius changes
        yield return new WaitForFixedUpdate();

        // Get random ray properties for raycast
        Vector3 rayDir = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f));
        rayDir = rayDir.normalized;
        float rayDistance = Random.Range(minSpawnRadius, maxSpawnRadius);

        // Shoot out raycast and check for collision
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(transform.position, rayDir, out hitInfo, rayDistance, lobLayerMask);
        Vector3 finalDestination = transform.position + (rayDistance * rayDir);

        if (hit) {
            finalDestination = hitInfo.point - (throwWallOffset * rayDir);
        }

        // Get ground height
        bool groundHit = Physics.Raycast(finalDestination, Vector3.down, out hitInfo, Mathf.Infinity, lobLayerMask);
        if (groundHit) {
            finalDestination.y = (hitInfo.point.y + groundOffset);
        }

        // Move indicator to final destination
        indicator.position = finalDestination;
        indicator.gameObject.SetActive(true);

        // Launch visual effect and wait for it to end
        lobVisualEffect.activateEffect(transform.position, finalDestination, dropDuration);
        yield return new WaitForSeconds(dropDuration);

        // Instantiate enemy in that area
        enemyUnit.transform.position = finalDestination;
        enemyUnit.transform.parent = null;
        enemyUnit.gameObject.SetActive(true);

        Object.Destroy(gameObject);
    }


    // Main function to set enemy patrol point
    public void setEnemyPatrolPoint(Transform[] patrolPoints) {
        PassivePatrolPointBranch enemyPatrolPoints = enemyUnit.GetComponent<PassivePatrolPointBranch>();

        if (enemyPatrolPoints != null) {
            enemyPatrolPoints.setPatrolPoints(patrolPoints);
        }
    }


    // Main function to set distances
    //  Pre: maxSpawnRadius >= minSpawnRadius >= 0
    //  Post: spawn radiuses are set
    public void setSpawnRadius(float min, float max) {
        Debug.Assert(min >= 0f && max >= min);

        minSpawnRadius = min;
        maxSpawnRadius = max;
    }


    // Main function to access the lobbedEnemy's enemy
    public IUnitStatus getAttachedEnemy() {
        return enemyUnit.GetComponent<IUnitStatus>();
    }
}
