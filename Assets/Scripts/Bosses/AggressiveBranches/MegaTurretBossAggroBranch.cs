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
    [SerializeField]
    private LobbedEnemy crushBotMinion;
    [SerializeField]
    private Transform[] arenaPatrolPoints;

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

    [SerializeField]
    private float minMinionSpawnRadius = 8f;
    [SerializeField]
    private float maxMinionSpawnRadius = 10f;
    [SerializeField]
    private float minionSpawnCooldown = 3.0f;
    private bool canSpawnMinion = true;
    private Coroutine runningSpawnCooldown = null;

    // Move probabilities that go in the following order: [basic projectile, laser, spawn enemy]
    [Header("Move Percent Chances")]
    [SerializeField]
    private float[] phase1MoveProbabilities;
    [SerializeField]
    private float[] phase2MoveProbabilities;
    [SerializeField]
    private float[] phase3MoveProbabilities;

    [Header("Phase 1")]
    [SerializeField]
    private float initialProjectileInterval = 0.45f;

    [Header("Phase 2")]
    [SerializeField]
    private float laterProjectileInterval = 0.35f;
    [SerializeField]
    private float initialLaserCharge = 0.9f;
    [SerializeField]
    private int initialMaxCrushBots = 1;

    [Header("Phase 3")]
    [SerializeField]
    private float triShotAngle = 35f;
    [SerializeField]
    private float lateLaserCharge = 0.75f;
    [SerializeField]
    private int lateMaxCrushBots = 2;

    // Crushbot handling
    private readonly object crushBotLock = new object();
    private HashSet<IUnitStatus> activeCrushBots = new HashSet<IUnitStatus>();
    private List<LobbedEnemy> lobbedEnemies = new List<LobbedEnemy>();


    // Enums concerning moves
    private enum MegaTurretMove {
        BASIC_ATTACK = 0,
        LASER = 1,
        SPAWN_MINION = 2
    };


    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize() {
        // Set up the lasers
        foreach (EnemyAreaOfEffect laser in lasers) {
            laser.changeColor(Color.clear);
        }

        errorCheckMoveProbabilities(phase1MoveProbabilities);
        errorCheckMoveProbabilities(phase2MoveProbabilities);
        errorCheckMoveProbabilities(phase3MoveProbabilities);
    }


    // Main function to error check probabilities
    private void errorCheckMoveProbabilities(float[] moveProbability) {
        // Make sure 3 moves are found in the move probabilities
        if (moveProbability.Length != 3) {
            Debug.LogError("Move probabilities do not match number of moves seen on this enemy. Moves should be in this format: [Basic ranged attack, laser, spawn crushbot]");
        }

        // Make sure total adds up to 1.0
        float curTotal = 0f;
        for (int i = 0; i < moveProbability.Length; i++) {
            curTotal += moveProbability[i];
        }

        if (curTotal < 0.99 || curTotal > 1.01) {
            Debug.LogError("The 3 move probabilities do not add up to 1.0. Moves should be in this format: [Basic ranged attack, laser, spawn crushbot]");
        }
    }


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt, int phaseNumber) {
        // Get attacking move for this current execution
        MegaTurretMove curAttack = chooseAttackingMove(phaseNumber);

        // Main tree based off of decision
        switch (curAttack) {
            // Basic attack
            case MegaTurretMove.BASIC_ATTACK:
                yield return shootBasicProjectile(tgt, phaseNumber);
                break;
            
            // Laser
            case MegaTurretMove.LASER:
                yield return fireLaser(tgt, phaseNumber);
                break;

            // Minion spawning
            case MegaTurretMove.SPAWN_MINION:
                yield return 0;
                spawnCrushBotMinion(phaseNumber);
                break;

            default:
                Debug.LogError("Current move not considered???: " + curAttack);
                break;
        }
    }


    // Main decision tree to choose attacking move to do on player
    //  Pre: 0 >= phaseNumber > 3
    //  Post: returns a move that the AI chooses based on phase number and RNG
    private MegaTurretMove chooseAttackingMove(int phaseNumber) {
        Debug.Assert(phaseNumber >= 0 && phaseNumber < 3);

        // Get the correct move probabilities diven the phase number
        float[] curMoveProbs = (phaseNumber == 0) ? phase1MoveProbabilities : phase2MoveProbabilities;
        if (phaseNumber == 2) {
            curMoveProbs = phase3MoveProbabilities;
        }

        Debug.Assert(curMoveProbs != null && curMoveProbs.Length > 0);

        // Record the total as the max dice value
        float maxDiceValue = 0f;
        foreach (float moveProb in curMoveProbs) {
            maxDiceValue += moveProb;
        }

        // Roll the dice
        float diceRoll = Random.Range(0f, maxDiceValue);
        int currentMove = 0;
        float curMoveThreshold = curMoveProbs[currentMove];

        // If diceRoll <= currentMoveThreshold, that move was picked
        while (diceRoll > curMoveThreshold && currentMove < curMoveProbs.Length) {
            // In case which currentMove was not picked, check the next move
            currentMove++;
            curMoveThreshold += curMoveProbs[currentMove];
        }

        Debug.Assert(currentMove >= 0 && currentMove < curMoveProbs.Length);

        // Return the mega turret move version of the int
        return (MegaTurretMove)currentMove;
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        // Clear out all lasers
        foreach (EnemyAreaOfEffect laser in lasers) {
            laser.changeColor(Color.clear);
        }
    }


    // Main event handler for when enemy has been despawn
    public override void hardReset() {
        // Do base reset
        reset();

        // Destroy all active crushbots
        lock (crushBotLock) {
            foreach (IUnitStatus crushBot in activeCrushBots) {
                crushBot.gameObject.SetActive(false);
            }

            activeCrushBots.Clear();
        }

        // If an enemy was in the process of being lobbed during this, destroy the lobbed enemy
        foreach (LobbedEnemy lob in lobbedEnemies) {
            if (lob != null) {
                lob.interrupt();
            }
        }

        lobbedEnemies.Clear();

        // reset cooldowns
        canSpawnMinion = true;
        canUseLaser = true;
        StopAllCoroutines();
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
        currentProjectile.setDamage(projDamage * enemyStats.getAttackMultiplier());
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

        if (canUseLaser) {
            // Aim at direction
            Vector3 aimDirection = getAimDirection(tgt);
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


    // Main function to spawn crush bot minion
    //  Pre: phaseNumber >= 0
    //  Post: spawns a random crushbot on the map
    private void spawnCrushBotMinion(int phaseNumber) {
        int curMaxBots = (phaseNumber >= 2) ? lateMaxCrushBots : initialMaxCrushBots;

        lock (crushBotLock) {
            if (activeCrushBots.Count < curMaxBots && canSpawnMinion) {
                // Set lobbing properties and enemy behavior
                LobbedEnemy currentMinion = Object.Instantiate(crushBotMinion, transform.position, Quaternion.identity);
                currentMinion.setSpawnRadius(minMinionSpawnRadius, maxMinionSpawnRadius);
                currentMinion.setEnemyPatrolPoint(arenaPatrolPoints);
                lobbedEnemies.Add(currentMinion);

                // Set up status
                IUnitStatus minionStatus = currentMinion.getAttachedEnemy();
                minionStatus.unitDeathEvent.AddListener(onCrushBotDeath);
                activeCrushBots.Add(minionStatus);

                if (activeCrushBots.Count < curMaxBots) {
                    runningSpawnCooldown = StartCoroutine(minionSpawnCooldownSequence());
                } else {
                    canSpawnMinion = false;
                }
            }
        }
    }


    // Main event handler function for when crushbot dies
    private void onCrushBotDeath(IUnitStatus corpse) {
        lock (crushBotLock) {
            if (activeCrushBots.Contains(corpse)) {
                activeCrushBots.Remove(corpse);

                if (runningSpawnCooldown == null) {
                    runningSpawnCooldown = StartCoroutine(minionSpawnCooldownSequence());
                }
            }
        }
    }


    // Minion spawn cooldown sequence
    private IEnumerator minionSpawnCooldownSequence() {
        canSpawnMinion = false;
        yield return new WaitForSeconds(minionSpawnCooldown);
        canSpawnMinion = true;

        runningSpawnCooldown = null;
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
