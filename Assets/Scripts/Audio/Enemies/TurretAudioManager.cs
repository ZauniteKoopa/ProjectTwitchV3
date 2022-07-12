using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAudioManager : EnemyAudioManager
{
    // Audio clips
    [SerializeField]
    private AudioClip chargeLaserClip;


    // Main function to set charging sound active
    //  Pre: bool to represent whether to turn charging on or off
    //  Post: plays charging
    public void setCharging(bool isCharging) {
        speaker.loop = isCharging;

        if (isCharging) {
            speaker.clip = chargeLaserClip;
            speaker.Play();
        }

    }
}
