using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class StationaryTurretAggroBranch : IEnemyAggroBranch
{
    [Header("Attack Sequence variables")]
    [SerializeField]
    private float surprisedDuration = 0.4f;
    [SerializeField]
    private float initialLaserAnticipation = 2.0f;
    [SerializeField]
    private float lateLaserAnticipation = 1.0f;
    [SerializeField]
    private float laserRechargeDuration = 0.5f;

    [Header("Laser properties")]
    [SerializeField]
    private Color fullLaserColor = Color.red;
    [SerializeField]
    private Color chargingLaserColor = Color.red;
    [SerializeField]
    private float lateLaserBlinkDuration = 0.1f;
    [SerializeField]
    private float laserSpeed = 45f;
    [SerializeField]
    private BasicEnemyProjectile projectile;
    private LineRenderer laserRender;
    private int curLaserBlinks = 0;


    // Main function to do additional initialization for branch
    //  Pre: none
    //  Post: sets branch up
    protected override void initialize() {
        laserRender = GetComponent<LineRenderer>();

        if (laserRender == null) {
            Debug.LogError("Line Renderer not attached to turret. Cannot render laser", transform);
        }

        setLaserColor(Color.clear);
        laserRender.enabled = false;
    }


    // Main function to execute the branch
    //  Pre: tgt is the player, cannot equal null
    //  Post: executes aggressive branch
    public override IEnumerator execute(Transform tgt) {
        // Surprised duration
        transform.forward = (tgt.position - transform.position).normalized;
        yield return new WaitForSeconds(surprisedDuration);

        // Main attacking loop
        while (true) {
            yield return chargeLaser(tgt);

            setLaserColor(Color.clear);
            fireLaser(tgt);
            yield return new WaitForSeconds(laserRechargeDuration);

        }
    }


    // Main private helper sequence to charge laser
    //  Pre: tgt != null
    //  Post: laser is doing its charging sequence
    private IEnumerator chargeLaser(Transform tgt) {
        Debug.Assert(tgt != null);

        // Set timer
        float timer = 0.0f;
        float totalAnticipation = initialLaserAnticipation + lateLaserAnticipation;
        WaitForFixedUpdate waitFrame = new WaitForFixedUpdate();

        // Initialize laser
        laserRender.enabled = true;
        Vector3[] laserPoints = new Vector3[] {transform.position, tgt.position};
        laserRender.SetPositions(laserPoints);
        setLaserColor(chargingLaserColor);
        curLaserBlinks = 0;

        // Anticipation loop
        while (timer < totalAnticipation) {
            yield return waitFrame;

            // If at late stage of laser anticipation, have laser blink
            if (timer >= lateLaserAnticipation) {
                // Get when the next blink is depending on how many blinks have passed already
                float nextBlink = lateLaserAnticipation + (curLaserBlinks * lateLaserBlinkDuration);

                // If you pass the point of nextBlink, change the color
                if (timer >= nextBlink) {
                    Color blinkColor = (curLaserBlinks % 2 == 0) ? fullLaserColor : chargingLaserColor;
                    setLaserColor(blinkColor);
                    curLaserBlinks++;
                }
            }

            // Have turret and laster always face the player
            laserPoints = new Vector3[] {transform.position, tgt.position};
            laserRender.SetPositions(laserPoints);
            transform.forward = (tgt.position - transform.position).normalized;

            // Update timer
            timer += Time.fixedDeltaTime;
        }
    }


    // Main function to fire the projectile
    //  Pre: none
    //  Post: fires a projectile at the direction of the target
    private void fireLaser(Transform target) {
        BasicEnemyProjectile currentProjectile = Object.Instantiate(projectile, transform.position, Quaternion.identity);
        currentProjectile.setDamage(enemyStats.getBaseAttack());
        currentProjectile.setUpMovement(target.position - transform.position, laserSpeed);
    }


    // Main function to reset the branch when the overall tree gets overriden / switch branches
    public override void reset() { 
        setLaserColor(Color.clear);
        laserRender.enabled = false;
    }


    // Main function to set the color
    //  Pre: laserRender != null
    //  Post: changes the color of the laser
    private void setLaserColor(Color color) {
        Debug.Assert(laserRender != null);

        laserRender.startColor = color; 
        laserRender.endColor = color;
    }
}
