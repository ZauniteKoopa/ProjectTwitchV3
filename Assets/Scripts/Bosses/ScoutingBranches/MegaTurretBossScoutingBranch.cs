using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaTurretBossScoutingBranch : IBossScoutingBranch
{
    [Header("Reference Prefabs")]
    [SerializeField]
    private BasicEnemyProjectile baseProjectile;
    [SerializeField]
    private EnemyAreaOfEffect[] lasers;
    [SerializeField]
    private Transform[] targetedPillars;

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
    [SerializeField]
    private float laserCooldown = 5f;
    private bool canUseLaser = true;

    [Header("Phase 1")]
    [SerializeField]
    private float initialProjectileInterval = 0.45f;

    [Header("Phase 2")]
    [SerializeField]
    private float laterProjectileInterval = 0.35f;
    [SerializeField]
    private float initialLaserCharge = 0.9f;
    [SerializeField]
    private float laserProbChance = 0.5f;

    [Header("Phase 3")]
    [SerializeField]
    private float lateLaserCharge = 0.75f;


    // On awake, error check
    private void Awake() {
        if (targetedPillars.Length <= 0) {
            Debug.LogError("No targeted pillars for boss to randomly target on last phase in scouting: " + transform, transform);
        }

        foreach (Transform pillar in targetedPillars) {
            if (pillar == null) {
                Debug.LogError("Found null element in targeted pillars list for scouting branch of this boss: " + transform, transform);
            }
        }
    }


    // Main function to run the branch
    public override IEnumerator execute(Vector3 lastSuspectedPlayerPos, int phaseNumber) {

        // Phase 0: Just basic fire at last supected location
        if (phaseNumber == 0) {
            yield return shootBasicProjectile(lastSuspectedPlayerPos, phaseNumber);

        // Phase 1: Random chance to either basic fire or shoot laser
        } else if (phaseNumber == 1) {
            float diceRoll = Random.Range(0f, 1f);

            if (diceRoll <= laserProbChance) {
                yield return fireTargetedLaser(lastSuspectedPlayerPos, phaseNumber, true);
            } else {
                yield return shootBasicProjectile(lastSuspectedPlayerPos, phaseNumber);
            }

        // Phase 2, just shoot lasers randomly
        } else {
            Transform curPillarTarget = targetedPillars[Random.Range(0, targetedPillars.Length)];
            yield return fireTargetedLaser(curPillarTarget.position, phaseNumber, false);
        }
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        // Clear out all lasers
        foreach (EnemyAreaOfEffect laser in lasers) {
            laser.changeColor(Color.clear);
        }
    }


    // Main event handler for when enemy has been despawn because of player death and you must clean up all side effects
    public override void hardReset() {
        reset();
        StopAllCoroutines();
        canUseLaser = true;
    }


    // Main IEnumerator sequence to shoot projectile
    //  Pre: phaseNumber >= 0 && tgt != null
    //  Post: waits a bit and then shoots a projectile
    private IEnumerator shootBasicProjectile(Vector3 suspectedTgt, int phaseNumber) {
        Debug.Assert(phaseNumber >= 0);

        float curProjAnticipation = (phaseNumber > 0) ? laterProjectileInterval : initialProjectileInterval;

        yield return new WaitForSeconds(curProjAnticipation);

        // Face aim direction and wait
        Vector3 aimDirection = getAimDirection(suspectedTgt);
        transform.forward = aimDirection;

        // Actually shoot projectile
        Vector3 projectileSrc = transform.position;
        projectileSrc.y = suspectedTgt.y;
        BasicEnemyProjectile currentProjectile = Object.Instantiate(baseProjectile, projectileSrc, Quaternion.identity);
        currentProjectile.setDamage(projDamage * enemyStats.getAttackMultiplier());
        currentProjectile.setUpMovement(aimDirection, projectileSpeed);
    }


    // Main IEnumerator to sequence to shoot laser
    //  Pre: main function to aim and charge a laser
    //  Post: Rotates to plays direction, waits to charge a laser and then attacks
    private IEnumerator fireTargetedLaser(Vector3 suspectedTgt, int phaseNumber, bool considerCooldown) {
        Debug.Assert(phaseNumber >= 0);

        if (canUseLaser || !considerCooldown) {
            // Aim at direction
            Vector3 aimDirection = getAimDirection(suspectedTgt);
            transform.forward = aimDirection;
            changeLaserColors(laserAnticipationColor, phaseNumber);
            float curAnticipation = (phaseNumber < 2) ? initialLaserCharge : lateLaserCharge;

            yield return new WaitForSeconds(curAnticipation);

            // Fire: change to fired color and do damage
            changeLaserColors(laserFiredColor, phaseNumber);
            
            if (phaseNumber < 2) {
                lasers[0].damageAllTargets(laserDamage * enemyStats.getAttackMultiplier());
            } else {
                foreach (EnemyAreaOfEffect laser in lasers) {
                    laser.damageAllTargets(laserDamage * enemyStats.getAttackMultiplier());
                }
            }

            // Clear everything up after a delay
            yield return new WaitForSeconds(0.15f);
            changeLaserColors(Color.clear, phaseNumber);

            // Start cooldown
            StartCoroutine(laserCooldownSequence());
        }
    }


    // Main sequence to handle laser cooldown
    private IEnumerator laserCooldownSequence() {
        canUseLaser = false;
        yield return new WaitForSeconds(laserCooldown);
        canUseLaser = true;
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
    private Vector3 getAimDirection(Vector3 suspectedTgt) {
        // Get random target position with same Y as transform.position
        Vector3 targetPosition = suspectedTgt;
        Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        randomDir = randomDir.normalized;
        targetPosition += (Random.Range(0f, aimRadius) * randomDir);
        targetPosition.y = transform.position.y;

        // Return that random direction from transform.position to target position
        return (targetPosition - transform.position).normalized;
    }

}
