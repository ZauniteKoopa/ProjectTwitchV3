using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class CrushbotAggroBranch : IEnemyAggroBranch
{
    [Header("Hitbox Variables")]
    [SerializeField]
    private EnemyBodyHitbox bodyHitbox;
    private bool hitPlayer = false;

    [Header("Navigation Sequence Variables")]
    [SerializeField]
    private float surprisedDuration = 0.5f;
    [SerializeField]
    private float pathExpirationInterval = 1.5f;

    [Header("Recoil Variables")]
    [SerializeField]
    private float recoilDistance = 1f;
    [SerializeField]
    private float recoilStunDuration = 2.5f;
    [SerializeField]
    private float recoilKnockbackDuration = 0.5f;

    // Audio
    private CrushBotAudioManager enemyAudio;


    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize() {
        // Error check
        if (bodyHitbox == null) {
            Debug.LogError("No body hitbox connected to crushbot!: " + transform, transform);
        }

        enemyAudio = GetComponent<CrushBotAudioManager>();
        if (enemyAudio == null) {
            Debug.LogError("No Crush Bot audio manager found with this crushbot", transform);
        }

        if (recoilStunDuration < 1f) {
            Debug.LogError("Recoil stun duration for crush bot should be more than 1 second for sound effects", transform);
        }

        // Hitbox
        bodyHitbox.init(enemyStats.getBaseAttack());
        bodyHitbox.damageTargetEvent.AddListener(onPlayerHitEnemy);
    }


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt) {
        // If this is the first time running this branch after override, look at enemy
        transform.forward = (tgt.position - transform.position).normalized;
        yield return new WaitForSeconds(surprisedDuration);

        // Go into chasing sequence that will last forever
        while (true) {
            hitPlayer = false;

            // While player not hit, keep chasing player
            while (!hitPlayer) {
                enemyAudio.setFootstepsActive(true);

                Vector3 currentDestination = tgt.position;
                yield return StartCoroutine(goToPosition(currentDestination, pathExpirationInterval));

                enemyAudio.setFootstepsActive(false);
            }

            // Once player has been hit, recoil
            enemyAudio.playAttackSound();
            Vector3 flattenPos = new Vector3(transform.position.x, tgt.position.y, transform.position.z);
            Vector3 recoilDir = (flattenPos - tgt.position).normalized;
            Vector3 recoilStart = transform.position;
            Vector3 recoilDest = transform.position + (recoilDistance * recoilDir);

            float recoilTimer = 0f;
            WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

            while (recoilTimer <= recoilKnockbackDuration) {
                yield return waitFrame;

                recoilTimer += Time.fixedDeltaTime;
                transform.position = Vector3.Lerp(recoilStart, recoilDest, recoilTimer / recoilKnockbackDuration);
            }

            // Enemy is stunned for fixed duration before moving to enemy again
            yield return new WaitForSeconds(recoilStunDuration - 1f);
            enemyAudio.playReactivateSound();
            yield return new WaitForSeconds(1f);
        }
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() {
        StopAllCoroutines();
        enemyAudio.setFootstepsActive(false);
    }


    // Main event handler function for when this enemy hits the player
    public void onPlayerHitEnemy() {
        hitPlayer = true;
    }


    // Main function to check for stop conditions when going to position
    //  Pre: none
    //  Post: returns whether a stop condition has been met
    protected override bool reachedStopCondition() {
        return hitPlayer;
    }
}
