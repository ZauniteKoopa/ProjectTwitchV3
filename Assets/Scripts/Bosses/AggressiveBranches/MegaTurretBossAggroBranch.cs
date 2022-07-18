using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MegaTurretBossAggroBranch : IBossAggroBranch
{
    [Header("Reference Prefabs")]
    [SerializeField]
    private BasicEnemyProjectile baseProjectile;
    [SerializeField]
    private EnemyAreaOfEffect[] lasers;


    [Header("General Variables")]
    [SerializeField]
    private float projectileSpeed = 12f;
    [SerializeField]
    private float aimRadius = 3f;
    [SerializeField]
    private float projDamage = 5f;
    [SerializeField]
    private float laserDamage = 8f;
    [SerializeField]
    private Color laserAnticipationColor = Color.yellow;
    [SerializeField]
    private Color laserFiredColor = Color.red;


    [Header("Phase 1")]
    [SerializeField]
    private float initialProjectileInterval = 0.45f;

    [Header("Phase 2")]
    [SerializeField]
    private float laterProjectileInterval = 0.35f;
    [SerializeField]
    private float initialLaserCharge = 0.9f;

    [Header("Phase 3")]
    [SerializeField]
    private float triShotAngle = 35f;
    [SerializeField]
    private float lateLaserCharge = 0.75f;

    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize() {
        // Set up the lasers
        foreach (EnemyAreaOfEffect laser in lasers) {
            laser.changeColor(Color.clear);
        }
    }


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt, int phaseNumber) {
        yield return fireLaser(tgt, phaseNumber);
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        // Clear out all lasers
        foreach (EnemyAreaOfEffect laser in lasers) {
            laser.changeColor(Color.clear);
        }
    }


    // Main IEnumerator sequence to shoot projectile
    //  Pre: phaseNumber >= 0 && tgt != null
    //  Post: waits a bit and then shoots a projectile
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


    // Main IEnumerator to sequence to shoot laser
    //  Pre: main function to aim and charge a laser
    //  Post: Rotates to plays direction, waits to charge a laser and then attacks
    private IEnumerator fireLaser(Transform tgt, int phaseNumber) {
        Debug.Assert(tgt != null && phaseNumber >= 0);

        // Aim at direction
        Vector3 aimDirection = getAimDirection(tgt);
        transform.forward = aimDirection;
        changeLaserColors(laserAnticipationColor, phaseNumber);
        float curAnticipation = (phaseNumber < 2) ? initialLaserCharge : lateLaserCharge;

        yield return new WaitForSeconds(curAnticipation);

        // Fire: change to fired color and do damage
        changeLaserColors(laserFiredColor, phaseNumber);
        
        if (phaseNumber < 2) {
            lasers[0].damageAllTargets(laserDamage);
        } else {
            foreach (EnemyAreaOfEffect laser in lasers) {
                laser.damageAllTargets(laserDamage);
            }
        }

        // Clear everything up after a delay
        yield return new WaitForSeconds(0.15f);
        changeLaserColors(Color.clear, phaseNumber);
    }


    // Main function to set laser colors
    //  Pre: phaseNumber >= 0
    //  Post: changes laser color accoridng to phase
    private void changeLaserColors(Color color, int phaseNumber) {
        // If only on phase 1, just do the direct one. Else, use the tri shot version
        if (phaseNumber < 2) {
            lasers[0].changeColor(color);
        } else {
            foreach (EnemyAreaOfEffect laser in lasers) {
                laser.changeColor(color);
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
