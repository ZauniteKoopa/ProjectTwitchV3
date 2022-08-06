using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class VFX_StatusEffectDisplay : MonoBehaviour
{
    // Main function to handle healing effects
    [SerializeField]
    private ParticleSystem healingParticles;

    [SerializeField]
    private Image invisScreen;
    [SerializeField]
    private float invisTransitionDuration = 0.5f;
    private Coroutine invisSequence = null;
    private Color invisColor;

    [SerializeField]
    private ParticleSystem manicEffect;

    [SerializeField]
    private CollapsingHalo impendingDoomHalo;

    private int healers = 0;


    // On awake, get variables
    private void Awake() {
        if (invisScreen != null) {
            invisColor = invisScreen.color;
        }
    }

    // Main function to display healing
    //  Pre: bool to represent whether to turn this on or off
    //  Post: displays healing if enemy is healing
    public void displayHealing(bool healing) {
        if (healingParticles != null) {
            int prevHealers = healers;
            healers += (healing) ? 1 : -1;

            // If first timehealing, play it. If all healers ended, stop healing
            if (prevHealers == 0) {
                healingParticles.Play();
            } else if (healers == 0){
                healingParticles.Stop();
            }
        }
    }


    // Main function to display manic
    //  Pre: bool to represent whether to turn this on or off
    //  Post: displays manic if enemy is manic
    public void displayManic(bool manic) {
        if (manicEffect != null) {
            if (manic) {
                manicEffect.Play();
            } else {
                manicEffect.Stop();
            }
        }
    }

    // Main function to display stealth
    //  Pre: bool to represent whether to turn this on or off
    //  Post: displays stealth if enemy is invisible
    public void displayStealth(bool invisible) {
        if (invisScreen != null) {
            // Stop current running invis sequence
            if (invisSequence != null) {
                StopCoroutine(invisSequence);
                invisSequence = null;
            }

            // If invisible, start a sequence, else, 
            if (invisible) {
                invisSequence = StartCoroutine(transitionInvisScreen());
            } else {
                invisScreen.gameObject.SetActive(false);
            }
        }
    }


    // Main function to transition invis screen
    private IEnumerator transitionInvisScreen() {
        float timer = 0f;
        invisScreen.gameObject.SetActive(true);
        invisScreen.color = Color.clear;

        while (timer < invisTransitionDuration) {
            yield return null;

            timer += Time.deltaTime;
            invisScreen.color = Color.Lerp(Color.clear, invisColor, timer / invisTransitionDuration);
        }

        invisScreen.color = invisColor;
        invisSequence = null;
    }


    // Main function to display doom halo
    //  Pre:
    //  Post:
    public void displayImpendingDoomHalo(float duration, Transform character) {
        if (impendingDoomHalo != null) {
            impendingDoomHalo.runCollapsingHalo(duration, character);
        }
    }


    // Main function to clear up everything
    //  Pre: none
    //  Post: clears side effects
    public void clear() {
        if (healingParticles != null) {
            healingParticles.Stop();
        }

        if (impendingDoomHalo != null) {
            impendingDoomHalo.clearHalo();
        }

        displayStealth(false);
        displayManic(false);
    }
}
