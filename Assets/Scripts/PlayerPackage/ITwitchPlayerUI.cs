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


    // Main function to display cask ammo cost
    //  Pre: ammoCost >= 0
    //  Post: cask ammo cost information is updated on UI
    public abstract void displayCaskAmmoCost(int ammoCost);


    // Main function to execute fade out sequence
    //  Pre: fadeColor is the solid color that you want to fade to, and duration is the time it takes to fade to that color
    //  Post: screen will fade to fadeColor in duration seconds
    public abstract void executeFadeOut(Color fadeColor, float duration);


    // Main function to display invisibility timer
    //  Pre: timeLeft <= maxTime  && 0 < maxTime && isVisible just references whether or not this timer should be visible
    //  Post: If isVisible is true, updates timer with current progress. Else, just disable timer
    public abstract void displayInvisibilityTimer(float timeLeft, float maxTime, bool isVisible);


    // Main function to display invisibility timer
    //  Pre: timeLeft <= maxTime  && 0 < maxTime && isVisible just references whether or not this timer should be visible
    //  Post: If isVisible is true, updates timer with current progress. Else, just disable timer
    public abstract void displayCraftingTimer(float timeLeft, float maxTime, bool isVisible);


    // Main function to display ability cooldown error
    //  Pre: None, player tried to use an ability when it was on cooldown
    //  Post: Notifies player that the ability they wanted to use was on cooldown
    public abstract void displayAbilityCooldownError();


    // Main function to display quick crafting error
    //  Pre: none, player tried to quick craft a vial that was already max stat
    //  Post: notifies player that they're quick crafting a max vial
    public abstract void displayQuickCraftingError();


    // Main function to display ability cooldown error
    //  Pre: None, player tried to use an ability when it was on cooldown
    //  Post: Notifies player that the ability they wanted to use was on cooldown
    public abstract void displayContaminateRangeError();
}
