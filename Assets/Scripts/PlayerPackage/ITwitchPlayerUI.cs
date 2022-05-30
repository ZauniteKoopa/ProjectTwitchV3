using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ITwitchPlayerUI : MonoBehaviour
{
    // Main function to update health bars of player
    //  Pre: curHealth <= maxHealth && maxHealth > 0
    //  Post: updates all health bars and other related to UI to match numbers
    public abstract void displayHealth(float curHealth, float maxHealth);


    // Main function to display information about primary vial
    //  Pre: primaryVial CAN either be non-null or null
    //  Post: updates primary vial related UI to reflect vial if not null or an empty vial
    public abstract void displayPrimaryVial(IVial primaryVial);

    
    // Main function to display information about secondary vial
    //  Pre: secondary vial can either be non-null OR null
    //  Post: updates secondary vial related UI to reflect vial if not null or an empty vial
    public abstract void displaySecondaryVial(IVial secondaryVial);


    // Main function to display camofladge cooldown status
    //  Pre: curCooldown <= maxCooldown && maxCooldown > 0
    //  Post: updates all camo cooldown related UI
    public abstract void displayCamoCooldown(float curCooldown, float maxCooldown);


    // Main function to display cask cooldown information
    //  Pre: curCooldown <= maxCooldown && maxCooldown > 0
    //  Post: updates all cask cooldown related UI
    public abstract void displayCaskCooldown(float curCooldown, float maxCooldown);


    // Main function to display contaminate cooldown information
    //  Pre: curCooldown <= maxCooldown && maxCooldown > 0
    //  Post: updates all contaminate cooldown UI
    public abstract void displayContaminateCooldown(float curCooldown, float maxCooldown);


    // Main function to display coins information
    //  Pre: numCoins >= 0
    //  Post: updates all coin related UI to match information
    public abstract void displayCoinsEarned(int numCoins);
}
