using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitchInventoryUIAudio : MonoBehaviour
{
    private AudioSource speaker;

    // Audio Clips to be used
    [Header("Audio clips")]
    [SerializeField]
    private AudioClip openInventorySound = null;
    [SerializeField]
    private AudioClip closeInventorySound = null;
    [SerializeField]
    private AudioClip dropElementSound = null;
    [SerializeField]
    private AudioClip grabElementSound = null;
    [SerializeField]
    private AudioClip[] craftErrorSounds = null;


    // Main function to get the speaker
    private void Awake() {
        speaker = GetComponent<AudioSource>();
        if (speaker == null) {
            Debug.LogError("No speaker connected to this object for TwitchInventoryUIAudio to connect to");
        }
    }


    // Private helper function to play audio clip from an array of clips
    private void playClip(AudioClip clip) {
        speaker.Stop();
        speaker.clip = clip;
        speaker.Play();
    }


    // Private helper function to play audio clip from an array of clips
    private void playRandomClip(AudioClip[] clips) {
        speaker.Stop();
        int clipIndex = Random.Range(0, clips.Length);
        speaker.clip = clips[clipIndex];
        speaker.Play();
    }


    // Function to play open sound
    public void playOpenSound() {
        playClip(openInventorySound);
    }


    // Function to play closed sound
    public void playClosedSound() {
        playClip(closeInventorySound);
    }


    // Function to play grab element sound
    public void playGrabSound() {
        playClip(grabElementSound);
    }


    // Function to play drop element sound
    public void playDropSound() {
        playClip(dropElementSound);
    }


    // Function to play craft error sound
    public void playCraftErrorSound() {
        playRandomClip(craftErrorSounds);
    }
}
