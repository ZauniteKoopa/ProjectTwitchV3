using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAudioManager : MonoBehaviour
{
    // Audio components
    protected AudioSource speaker = null;

    [Header("Footsteps")]
    [SerializeField]
    private FootstepsManager footstepsManager;
    [SerializeField]
    private float defaultTimePerStep = 0.25f;
    [SerializeField]
    private bool isStationary = false;

    [Header("Generic Sounds")]
    [SerializeField]
    private AudioClip[] deathSounds = null;
    [SerializeField]
    private AudioClip[] attackSounds = null;

    // On awake, get access to audio source
    private void Awake() {
        speaker = GetComponent<AudioSource>();

        if (speaker == null) {
            Debug.LogError("Audio Source not connected to " + transform + " for AudioManager to make use of");
        }

        if (footstepsManager == null) {
            if (!isStationary) {
                Debug.LogWarning("No footsteps manager connected to enemy. Enemy will not make footsteps. Either attach footsteps object as a child or mark enemy as stationary", transform);
            }
        } else {
            footstepsManager.setTimeStep(defaultTimePerStep);
        }
    }


    // Private helper function to play audio clip from an array of clips
    protected void playRandomClip(AudioClip[] clips) {
        speaker.Stop();
        speaker.loop = false;
        int clipIndex = Random.Range(0, clips.Length);
        speaker.clip = clips[clipIndex];
        speaker.Play();
    }


    // Public method to play death sounds
    public void playDeathSound() {
        playRandomClip(deathSounds);
    }


    // Public method to play attack sounds
    public void playAttackSound() {
        playRandomClip(attackSounds);
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
