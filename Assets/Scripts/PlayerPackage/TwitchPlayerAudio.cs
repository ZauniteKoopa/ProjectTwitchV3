using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TwitchPlayerAudio : MonoBehaviour
{
    // Audio components
    private AudioSource baseSpeaker = null;
    [SerializeField]
    private AudioSource voiceSpeaker = null;

    // Audio Clips to be used
    [Header("Audio clips")]
    [SerializeField]
    private AudioClip[] fireBoltSounds = null;
    [SerializeField]
    private AudioClip[] contaminateSounds = null;
    [SerializeField]
    private AudioClip[] stealthCastSounds = null;
    [SerializeField]
    private AudioClip[] caskThrowSounds = null;
    [SerializeField]
    private AudioClip[] hurtSounds = null;
    [SerializeField]
    private AudioClip[] deathSounds = null;
    [SerializeField]
    private AudioClip[] errorSounds = null;
    [SerializeField]
    private AudioClip[] pickupSounds = null;
    [SerializeField]
    private AudioClip[] basicUpgradeSounds = null;
    [SerializeField]
    private AudioClip[] sideEffectUpgradeSounds = null;
    [SerializeField]
    private AudioClip vialMixingSound = null;


    [Header("Footsteps")]
    [SerializeField]
    FootstepsManager footstepsManager;
    [SerializeField]
    float defaultTimePerStep = 0.25f;


    // On awake, get access to audio source
    private void Awake() {
        baseSpeaker = GetComponent<AudioSource>();

        if (baseSpeaker == null) {
            Debug.LogError("Audio Source not connected to " + transform + " for AudioManager to make use of");
        }

        if (voiceSpeaker == null) {
            Debug.LogError("Voice Speaker not connected to player");
        }

        if (footstepsManager == null) {
            Debug.LogWarning("No footsteps manager connected to player. Player will not make footsteps", transform);
        } else {
            footstepsManager.setTimeStep(defaultTimePerStep);
        }
    }


    // Private helper function to play audio clip from an array of clips
    private void playRandomClip(AudioClip[] clips, AudioSource speaker) {
        speaker.Stop();
        speaker.loop = false;
        int clipIndex = Random.Range(0, clips.Length);
        speaker.clip = clips[clipIndex];
        speaker.Play();
    }


    // Public method used to play firing bolt sound
    public void playBoltSound() {
        playRandomClip(fireBoltSounds, baseSpeaker);
    }


    // Public method to play contaminate sound
    public void playContaminateSound() {
        playRandomClip(contaminateSounds, baseSpeaker);
    }


    // Public method to play stealth cast sound
    public void playStealthCastSound() {
        playRandomClip(stealthCastSounds, baseSpeaker);
    }


    // Public method to play cask throw sounds
    public void playCaskCastSound() {
        playRandomClip(caskThrowSounds, baseSpeaker);
    }

    // Public method to play hurt sounds
    public void playHurtSound() {
        playRandomClip(hurtSounds, voiceSpeaker);
    }

    // Public method to play death sounds
    public void playDeathSound() {
        playRandomClip(deathSounds, voiceSpeaker);
    }


    // Public method to player error sounds
    public void playErrorSound() {
        playRandomClip(errorSounds, baseSpeaker);
    }


    // Main function to play pickup sounds
    public void playPickUpSound() {
        playRandomClip(pickupSounds, baseSpeaker);
    }


    // Main function for making upgrade sounds
    public void playBasicUpgradeSound() {
        playRandomClip(basicUpgradeSounds, baseSpeaker);
    }


    // Main function to play side effect upgrade sound
    public void playSideEffectUpgradeSound() {
        playRandomClip(sideEffectUpgradeSounds, voiceSpeaker);
    }


    // Main function to play vial mixing sound
    //  Pre: isActive represents whether to set vial mixing on or off
    public void setVialMixing(bool isActive) {
        baseSpeaker.Stop();
        baseSpeaker.loop = isActive;

        if (isActive) {
            baseSpeaker.clip = vialMixingSound;
            baseSpeaker.Play();
        }
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
