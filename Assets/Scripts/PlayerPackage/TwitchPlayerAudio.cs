using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitchPlayerAudio : MonoBehaviour
{
    // Audio components
    private AudioSource speaker = null;

    // Audio Clips to be used
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


    // On awake, get access to audio source
    private void Awake() {
        speaker = GetComponent<AudioSource>();

        if (speaker == null) {
            Debug.LogError("Audio Source not connected to " + transform + " for AudioManager to make use of");
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
}
