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

    // Private instance variables
    private IVial vial;


    //Method to set up vial with disabled or enabled in mind
    public void DisplayVial(IVial pv)
    {
        vial = pv;
        
        //Enable or disable
        if (pv != null)
        {
            ShowEnabled();
            vialDisplay.color = pv.getColor();
        }
        else
        {
            ShowDisabled();
        }

        // Get ammo information
        float displayedAmmo = (pv == null) ? 0 : pv.getAmmoLeft();
        float maxAmmo = (pv == null) ? 0 : pv.getMaxVialSize();

        // Update Ammo bars
        foreach (ResourceBar ammoBar in ammoBars) {
            ammoBar.setStatus(displayedAmmo, maxAmmo);
        }

        // Update stat information IFF stat displays are available
        if (potencyStatDisplay != null) {
            if (pv == null) {
                potencyStatDisplay.displayNumber(0);
                poisonStatDisplay.displayNumber(0);
                reactivityStatDisplay.displayNumber(0);
                stickinessStatDisplay.displayNumber(0);
            } else {
                Dictionary<string, int> vialStats = pv.getStats();

                potencyStatDisplay.displayNumber(vialStats["Potency"]);
                poisonStatDisplay.displayNumber(vialStats["Poison"]);
                reactivityStatDisplay.displayNumber(vialStats["Reactivity"]);
                stickinessStatDisplay.displayNumber(vialStats["Stickiness"]);
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
