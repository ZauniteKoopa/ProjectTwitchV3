using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegaTurretBossAudioManager : BossAudioManager
{
    [Header("Mega Turret Sounds")]
    [SerializeField]
    private AudioClip[] laserChargeSounds;
    [SerializeField]
    private AudioClip[] laserFireSounds;
    [SerializeField]
    private AudioClip[] bulletFireSounds;


    // Main functions to play audio clips
    public void playLaserCharge() {
        playRandomClip(laserChargeSounds);
    }


    // Main functions to play audio clips
    public void playLaserFire() {
        playRandomClip(laserFireSounds);
    }


    // Main functions to play audio clips
    public void playBulletFire() {
        playRandomClip(bulletFireSounds);
    }
}
