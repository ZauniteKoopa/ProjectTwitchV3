using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InventoryHoverPopupDisplays : MonoBehaviour
{
    [SerializeField]
    private TMP_Text[] potencyTexts;
    [SerializeField]
    private TMP_Text[] poisonTexts;
    [SerializeField]
    private TMP_Text[] reactivityTexts;
    [SerializeField]
    private TMP_Text[] stickinessTexts;
    private const float WEAK_BOLT_DAMAGE = 1.75f;


    // Main function to display information about a primary vial
    //  Pre: displayed vial can be either null or non-null
    //  Post: updates all stat text
    public void updateDisplays(IVial displayedVial) {
        // Display potency text
        foreach (TMP_Text potencyText in potencyTexts) {
            potencyText.text = (displayedVial != null) ? 
                "<color=red>Potency</color> controls immediate bolt damage and cask damage. Bolts do <color=red>"+ displayedVial.getBoltDamage(0) +"</color> damage and casks do <color=red>" + (2f * displayedVial.getBoltDamage(0)) +"</color> damage" :
                "<color=red>Potency</color> controls immediate bolt damage and cask damage. With no poison equipped, bolts only do <color=red>" + WEAK_BOLT_DAMAGE + "</color> damage";
        }
        

        // Display poison text
        foreach (TMP_Text poisonText in poisonTexts) {
            if (displayedVial != null) {
                float decayRate = Mathf.Round(displayedVial.getPoisonDecayRate() * 100f) / 100f;
                float simplePoisonDamage = displayedVial.getPoisonDamage(1);
                float maxPoisonDamage = displayedVial.getPoisonDamage(6);

                poisonText.text = "<color=red>Poison</color> controls damage per tick on an infected enemy. Each tick lasts <color=red>"+ decayRate +" seconds</color>, dealing <color=red>"+ simplePoisonDamage +" true damage for each stack</color>, capping at <color=red>"+ maxPoisonDamage +"</color>. True damage ignores armor";
            } else {
                poisonText.text = "<color=red>Poison</color> controls damage per tick on an infected enemy. With no poison equipped, bolts <color=red>cannot infect enemies</color>";
            }
        }

        // Display reactivity text
        foreach (TMP_Text reactivityText in reactivityTexts) {
            reactivityText.text = (displayedVial != null) ?
                "<color=red>Reactivity</color> controls Contaminate. Upon using Contaminate, units infected by this poison receive <color=red>"+ displayedVial.getContaminateDamage(1) +"</color> damage at 1 stack, <color=red>"+ displayedVial.getContaminateDamage(6) +"</color> damage at max stacks. <color=red>If this kills, reset stealth</color>" :
                "<color=red>Reactivity</color> controls Contaminate. With no poison equipped, bolts <color=red>cannot infect enemies</color>. Contaminate <color=red>only affects infected enemies</color>";
        }
        

        // Display stickiness text
        foreach(TMP_Text stickinessText in stickinessTexts) {
            if (displayedVial != null) {
                float stackSlowness = (1.0f - displayedVial.getStackSlowness(6)) * 100f;
                float caskSlowness = (1.0f - displayedVial.getCaskSlowness()) * 100f;

                stackSlowness = Mathf.Round(stackSlowness * 10f) / 10f;
                caskSlowness = Mathf.Round(caskSlowness * 10f) / 10f;

                stickinessText.text = "<color=red>Stickiness</color> handles crowd control. Enemy infected with this poison will slow gradually, slowing by <color=red>"+ stackSlowness +"% at max stacks</color>. Cask zones also become sticky, slowing units by <color=red>"+ caskSlowness +"%</color>";
            } else {
                stickinessText.text = "<color=red>Stickiness</color> handles crowd control. With no poison equipped, <color=red>you cannot throw casks or infect enemies to slow them down</color>";
            }
        }

    }
}
