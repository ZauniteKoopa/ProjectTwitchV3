using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VialIcon : AbilityIcon
{
    //UI reference variables
    [SerializeField]
    private ResourceBar[] ammoBars;
    [SerializeField]
    private INumberDisplay potencyStatDisplay;
    [SerializeField]
    private INumberDisplay poisonStatDisplay;
    [SerializeField]
    private INumberDisplay reactivityStatDisplay;
    [SerializeField]
    private INumberDisplay stickinessStatDisplay;
    [SerializeField]
    private Image vialDisplay;
    [SerializeField]
    private ResourceBar totalStats;
    [SerializeField]
    private TMP_Text sideEffectName;
    [SerializeField]
    private TMP_Text sideEffectDescription;

    // Private instance variables
    private IVial vial;


    //Method to set up vial with disabled or enabled in mind
    public void DisplayVial(IVial pv)
    {
        vial = pv;
        Color vialColor = (pv == null) ? Color.black : pv.getColor();
        
        //Enable or disable
        if (vialDisplay != null) {
            vialDisplay.color = vialColor;
        }

        // Get ammo information
        float displayedAmmo = (pv == null) ? 0 : pv.getAmmoLeft();
        float maxAmmo = (pv == null) ? 60 : pv.getMaxVialSize();

        // Get side effect info
        Specialization vialSpecialization;
        string[] sideEffectInfo = pv.getSideEffectInfo(out vialSpecialization);

        // Update Ammo bars
        foreach (ResourceBar ammoBar in ammoBars) {
            ammoBar.setStatus(displayedAmmo, maxAmmo);
            ammoBar.setColor(vialColor);
        }

        displayStats(pv, vialSpecialization);

        // If totalStats bar given, update information
        if (totalStats != null) {
            float currentTotalStat = (pv == null) ? 0 : pv.getCurrentTotalStat();
            float currentMaxStat = (pv == null) ? 10 : pv.getMaxTotalStat();
            totalStats.setStatus(currentTotalStat, currentMaxStat);
        }

        // Edit side effect display info if it exists
        if (sideEffectName != null) {
            sideEffectName.text = "Side Effect: " + sideEffectInfo[0];
        }

        if (sideEffectDescription != null) {
            sideEffectDescription.text = sideEffectInfo[1];
        }
    }
    

    // Private helper function for displaying information in stat displays
    //  Pre: none
    //  Post: update stat labels with appropriate info
    private void displayStats(IVial pv, Specialization specialization) {
        if (potencyStatDisplay != null) {
            // Display numbers
            Dictionary<string, int> vialStats = (pv == null) ? null : pv.getStats();
            int potency = (pv == null) ? 0 : vialStats["Potency"];
            int poison = (pv == null) ? 0 : vialStats["Poison"];
            int reactivity = (pv == null) ? 0 : vialStats["Reactivity"];
            int stickiness = (pv == null) ? 0 : vialStats["Stickiness"];

            
            potencyStatDisplay.displayNumber(potency);
            poisonStatDisplay.displayNumber(poison);
            reactivityStatDisplay.displayNumber(reactivity);
            stickinessStatDisplay.displayNumber(stickiness);

            // Display colors
            if (pv != null) {
                Color potencyColor = (specialization != Specialization.POTENCY) ? Color.black : Color.red;
                Color poisonColor = (specialization != Specialization.POISON) ? Color.black : Color.red;
                Color reactivityColor = (specialization != Specialization.REACTIVITY) ? Color.black : Color.red;
                Color stickinessColor = (specialization != Specialization.STICKINESS) ? Color.black : Color.red;

                potencyStatDisplay.displayColor(potencyColor);
                poisonStatDisplay.displayColor(poisonColor);
                reactivityStatDisplay.displayColor(reactivityColor);
                stickinessStatDisplay.displayColor(stickinessColor);
            } else {
                potencyStatDisplay.displayColor(Color.black);
                poisonStatDisplay.displayColor(Color.black);
                reactivityStatDisplay.displayColor(Color.black);
                stickinessStatDisplay.displayColor(Color.black);
            }
        }
    }

    //Method to access the UI sprite for this icon
    public Sprite GetSprite()
    {
        return vialDisplay.sprite;
    }

    //Method to access poison vial
    public IVial GetVial()
    {
        return vial;
    }
}
