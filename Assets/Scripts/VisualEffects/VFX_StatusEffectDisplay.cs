using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VFX_StatusEffectDisplay : MonoBehaviour
{
    // Main function to handle healing effects
    [SerializeField]
    private ParticleSystem healingParticles;
    [SerializeField]
    private Image invisScreen;
    [SerializeField]
    private ParticleSystem manicEffect;

    private int healers = 0;


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
            invisScreen.gameObject.SetActive(invisible);
        }
    }


    // Main function to clear up everything
    //  Pre: none
    //  Post: clears side effects
    public void clear() {
        if (healingParticles != null) {
            healingParticles.Stop();
        }

        displayStealth(false);
        displayManic(false);
    }
}
