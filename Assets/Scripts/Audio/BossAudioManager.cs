using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAudioManager : EnemyAudioManager
{
    [Header("Boss Specific Sounds")]
    [SerializeField]
    private AudioClip[] phaseShiftSounds = null;


    // Main function to play phase sounds
    public void playPhaseShiftSound() {
        playRandomClip(phaseShiftSounds);
    }

}
