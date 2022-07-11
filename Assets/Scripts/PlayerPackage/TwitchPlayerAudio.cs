using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TwitchPlayerAudio : MonoBehaviour
{
    // Audio components
    private AudioSource speaker = null;

    // Audio Clips to be used
    [Header("Audio clips")]
    [SerializeField]
    AudioClip[] fireBoltSounds = null;
    [SerializeField]
    AudioClip[] contaminateSounds = null;
    [SerializeField]
    AudioClip[] stealthCastSounds = null;
    [SerializeField]
    AudioClip[] caskThrowSounds = null;
    [SerializeField]
    AudioClip[] hurtSounds = null;
    [SerializeField]
    AudioClip[] deathSounds = null;
    [SerializeField]
    AudioClip[] errorSounds = null;

    [Header("Footsteps")]
    [SerializeField]
    FootstepsManager footstepsManager;
    [SerializeField]
    float defaultTimePerStep = 0.25f;


    // On awake, get access to audio source
    private void Awake() {
        speaker = GetComponent<AudioSource>();

        if (speaker == null) {
            Debug.LogError("Audio Source not connected to " + transform + " for AudioManager to make use of");
        }

        if (footstepsManager == null) {
            Debug.LogWarning("No footsteps manager connected to player. Player will not make footsteps", transform);
        } else {
            footstepsManager.setTimeStep(defaultTimePerStep);
        }
    }


    // Private helper function to play audio clip from an array of clips
    private void playRandomClip(AudioClip[] clips) {
        int clipIndex = Random.Range(0, clips.Length);
        speaker.clip = clips[clipIndex];
        speaker.Play();
    }


    // Public method used to play firing bolt sound
    public void playBoltSound() {
        playRandomClip(fireBoltSounds);
    }


    // Public method to play contaminate sound
    public void playContaminateSound() {
        playRandomClip(contaminateSounds);
    }


    // Public method to play stealth cast sound
    public void playStealthCastSound() {
        playRandomClip(stealthCastSounds);
    }


    // Public method to play cask throw sounds
    public void playCaskCastSound() {
        playRandomClip(caskThrowSounds);
    }

    // Public method to play hurt sounds
    public void playHurtSound() {
        playRandomClip(hurtSounds);
    }

    // Public method to play death sounds
    public void playDeathSound() {
        playRandomClip(deathSounds);
    }


    // Public method to player error sounds
    public void playErrorSound() {
        playRandomClip(errorSounds);
    }

    // Main function to set step rate
    //  Pre: speedFactor > 0f. If less than 1, slows unit down. If more than 1, unit is faster
    //  Post: sets speed factor for calculation
    public void setStepRateFactor(float speedFactor) {
        Debug.Assert(speedFactor > 0f);

        if (footstepsManager != null) {

            // If you go faster, time per step decreases. If you go slower, time per step increases
            footstepsManager.setTimeStep(defaultTimePerStep / speedFactor);
        }
    }


    // Main function to set Footstep active status: This is AN EVENT HANDLER. Do not play this constantly
    //  Pre: boolean to represent whether to make this active or not
    //  Post: If true, plays footsteps. If false, stops footstep
    public void setFootstepsActive(bool isActive) {
        if (footstepsManager != null) {
            if (isActive) {
                footstepsManager.play();
            } else {
                footstepsManager.stop();
            }
        }
    }
}
