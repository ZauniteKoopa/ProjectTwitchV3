using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MegaTurretBossAggroBranch : IBossAggroBranch
{
    [Header("Reference Prefabs")]
    [SerializeField]
    private BasicEnemyProjectile baseProjectile;


    [Header("General Variables")]
    [SerializeField]
    private float projectileSpeed = 12f;
    [SerializeField]
    private float aimRadius = 3f;
    [SerializeField]
    private float projDamage = 5f;


    [Header("Phase 1")]
    [SerializeField]
    private float initialProjectileInterval = 0.45f;

    [Header("Phase 2")]
    [SerializeField]
    private float laterProjectileInterval = 0.35f;

    [Header("Phase 3")]
    [SerializeField]
    private float triShotAngle = 35f;

    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize() {

    }


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt, int phaseNumber) {
        yield return shootBasicProjectile(tgt, phaseNumber);
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {

    }


    // Main IEnumerator sequence to shoot projectile
    //  Pre: phaseNumber >= 0 && tgt != null
    private IEnumerator shootBasicProjectile(Transform tgt, int phaseNumber) {
        Debug.Assert(tgt != null && phaseNumber >= 0);

        float curProjAnticipation = (phaseNumber > 0) ? laterProjectileInterval : initialProjectileInterval;

        yield return new WaitForSeconds(curProjAnticipation);

        // Face aim direction and wait
        Vector3 aimDirection = getAimDirection(tgt);
        transform.forward = aimDirection;

        // Actually shoot projectile
        Vector3 projectileSrc = transform.position;
        projectileSrc.y = tgt.position.y;
        BasicEnemyProjectile currentProjectile = Object.Instantiate(baseProjectile, projectileSrc, Quaternion.identity);
        currentProjectile.setDamage(projDamage);
        currentProjectile.setUpMovement(aimDirection, projectileSpeed);

        // If you're in Phase Number 2, do triple shots
        if (phaseNumber >= 2) {
            Vector3 angledDir1 = Quaternion.AngleAxis(triShotAngle, Vector3.up) * aimDirection;
            Vector3 angledDir2 = Quaternion.AngleAxis(-triShotAngle, Vector3.up) * aimDirection;

            for (int p = 0; p < 2; p++) {
                Vector3 curAngledDir = (p == 0) ? angledDir1 : angledDir2;

                currentProjectile = Object.Instantiate(baseProjectile, projectileSrc, Quaternion.identity);
                currentProjectile.setDamage(projDamage);
                currentProjectile.setUpMovement(curAngledDir, projectileSpeed);
            }
        }

    }


    // Main function to get aim direction given a target
    //  Pre: tgt != null
    //  Post: gets a direction that aims at THAT general area (not always direct)
    private Vector3 getAimDirection(Transform tgt) {
        Debug.Assert(tgt != null);

        // Get random target position with same Y as transform.position
        Vector3 targetPosition = tgt.position;
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        randomDir = randomDir.normalized;
        targetPosition += (Random.Range(0f, aimRadius) * randomDir);
        targetPosition.y = transform.position.y;

        // Return that random direction from transform.position to target position
        return (targetPosition - transform.position).normalized;
    }
}
