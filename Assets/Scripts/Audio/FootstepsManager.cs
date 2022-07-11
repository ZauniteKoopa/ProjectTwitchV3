using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class FootstepsManager : MonoBehaviour
{
    // Private instance variables
    private float timePerStep = 1f;
    private Coroutine currentFootstepSequence = null;
    private AudioSource speaker;

    // Audio clips for footsteps
    [SerializeField]
    private AudioClip[] footstepSounds;


    // Main function to initialize on awake
    private void Awake() {
        // Get reference variables
        speaker = GetComponent<AudioSource>();

        if (speaker == null) {
            Debug.LogError("No speaker found on this gameobject for footsteps manager to utilize", transform);
        }

        // If no audio clip found, debug error
        if (footstepSounds.Length == 0) {
            Debug.LogError("No footsteps sound in footstep manager. What sounds do I even play??", transform);
        }
    }


    // Main function to play footsteps with this speed
    //  Pre: none
    //  Post: plays footstep sequence at given stepTime
    public void play() {
        Debug.Assert(timePerStep > 0f);

        // If sequence isn't running yet, set sequence
        if (currentFootstepSequence == null) {
            currentFootstepSequence = StartCoroutine(footstepSequence());
        }
    }


    // Main function to stop footstep sequence
    //  Pre: none
    //  Post: stops footsteps coroutine so it doesn't make any sounds now
    public void stop() {
        if (currentFootstepSequence != null) {
            // Get rid of footstep sequence
            StopCoroutine(currentFootstepSequence);
            currentFootstepSequence = null;

            // Silence audio 
            speaker.Stop();
        }
    }


    // Main function to set rate without having to start sequence
    //  Pre: timeStep > 0f
    //  Post: timeStep is set
    public void setTimeStep(float timeStep) {
        Debug.Assert(timeStep > 0f);

        timePerStep = timeStep;
    }


    // Private IEnumerator to do footsteps sequence
    //  Pre: timePerStep is set to a value > 0f, there's at least one sound in footstepSounds
    //  Post: sequence will play sound effect constantly 
    private IEnumerator footstepSequence() {

        while (true) {
            // Wait for time step before playing footsteps
            Debug.Assert(timePerStep > 0f);
            yield return new WaitForSeconds(timePerStep);

            // Choose an audio clip and play it
            Debug.Assert(footstepSounds.Length > 0);
            AudioClip curClip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            Debug.Assert(curClip != null);

            speaker.clip = curClip;
            speaker.Play();
        }
    }
}
