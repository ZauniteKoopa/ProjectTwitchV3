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

        // Update Ammo bars
        foreach (ResourceBar ammoBar in ammoBars) {
            ammoBar.setStatus(displayedAmmo, maxAmmo);
            ammoBar.setColor(vialColor);
        }

        // Update stat information IFF stat displays are available
        if (potencyStatDisplay != null) {
            Dictionary<string, int> vialStats = (pv == null) ? null : pv.getStats();
            int potency = (pv == null) ? 0 : vialStats["Potency"];
            int poison = (pv == null) ? 0 : vialStats["Poison"];
            int reactivity = (pv == null) ? 0 : vialStats["Reactivity"];
            int stickiness = (pv == null) ? 0 : vialStats["Stickiness"];

            potencyStatDisplay.displayNumber(potency);
            poisonStatDisplay.displayNumber(poison);
            reactivityStatDisplay.displayNumber(reactivity);
            stickinessStatDisplay.displayNumber(stickiness);
        }

        // If totalStats bar given, update information
        if (totalStats != null) {
            float currentTotalStat = (pv == null) ? 0 : pv.getCurrentTotalStat();
            float currentMaxStat = (pv == null) ? 10 : pv.getMaxTotalStat();
            totalStats.setStatus(currentTotalStat, currentMaxStat);
        }

        // Get side effect information
        string[] sideEffectInfo = pv.getSideEffectInfo();
        if (sideEffectName != null) {
            sideEffectName.text = "Side Effect: " + sideEffectInfo[0];
        }

        if (sideEffectDescription != null) {
            sideEffectDescription.text = sideEffectInfo[1];
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
