using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrushBotAudioManager : EnemyAudioManager
{
    [Header("Crush Bot Specific Sounds")]
    [SerializeField]
    private AudioClip[] reactivateSound;

    // Main function to play reactivate sound
    public void playReactivateSound() {
        playRandomClip(reactivateSound);
    }


}
