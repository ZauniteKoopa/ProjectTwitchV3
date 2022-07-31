using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;

public class StatusEffectDisplay : MonoBehaviour
{
    // Stuff for the speed status effect
    [SerializeField]
    private Image speedIcon;
    [SerializeField]
    private TMP_Text speedText;

    // Stuff for the stunned side effect
    [SerializeField]
    private Image stunIcon;
    
    // Stuff for the Manic side effect
    [SerializeField]
    private Image manicIcon;

    // Stuff for the volatile side effect
    [SerializeField]
    private Image volatileIcon;

    // Stuff for the invisibility icon
    [SerializeField]
    private Image invisibilityIcon;
    [SerializeField]
    private ResourceBar invisibilityBar;

    // Stuff for healing icon
    [SerializeField]
    private Image healingIcon;


    [SerializeField]
    private float deltaX = 10f;
    private List<Image> activeStatusEffects = new List<Image>();
    private readonly object iconOrderLock = new object();


    // On awake, disable everything
    private void Awake() {
        clear();
    }


    // Main function to check speed icon
    //  Pre: speedModifier > 0.0f
    //  Post: displays the speed status buff accordingly
    public void displaySpeedStatus(float speedModifier) {
        Debug.Assert(speedModifier > 0.0f);

        if (speedIcon != null) {
            // Show speed displays IFF speed was modified
            bool speedModified = speedModifier <= 0.9f || speedModifier >= 1.1f;
            bool prevSpeedModified = speedIcon.gameObject.activeInHierarchy;

            // Only change sort order if prevSpeedModified != speedModified (a change actually occurred) to reduce cost
            if (prevSpeedModified != speedModified) {
                changeSortOrder(speedIcon, speedModified);
            }

            speedIcon.gameObject.SetActive(speedModified);
            speedText.gameObject.SetActive(speedModified);

            // If speed was modified, update icons accordingly
            if (speedModified) {
                speedIcon.color = (speedModifier <= 0.9f) ? Color.red : Color.green;
                float roundedModifier = Mathf.Round(speedModifier * 10f) / 10f;
                speedText.text = roundedModifier + "x";
            }
        }
    }


    // Main function to display stun
    //  Pre: bool to represent whether to turn this on or off
    //  Post: displays stun if enemy is stunned
    public void displayStun(bool stunned) {
        if (stunIcon != null) {
            bool prevStunned = stunIcon.gameObject.activeInHierarchy;

            // Only change sort order if stun status changed to reduce cost
            if (prevStunned != stunned) {
                changeSortOrder(stunIcon, stunned);
            }

            stunIcon.gameObject.SetActive(stunned);
        }
    }


    // Main function to display volatile
    //  Pre: bool to represent whether to turn this on or off
    //  Post: displays volatile if enemy is volatile
    public void displayVolatile(bool willVolatile) {
        if (volatileIcon != null) {
            bool prevVolatile = volatileIcon.gameObject.activeInHierarchy;

            // Only change sort order if manic status changed to reduce cost
            if (prevVolatile != willVolatile) {
                changeSortOrder(volatileIcon, willVolatile);
            }

            volatileIcon.gameObject.SetActive(willVolatile);
        }
    }


    // Main function to display manic
    //  Pre: bool to represent whether to turn this on or off
    //  Post: displays manic if enemy is manic
    public void displayManic(bool manic) {
        if (manicIcon != null) {
            bool prevManic = manicIcon.gameObject.activeInHierarchy;

            // Only change sort order if manic status changed to reduce cost
            if (prevManic != manic) {
                changeSortOrder(manicIcon, manic);
            }

            manicIcon.gameObject.SetActive(manic);
        }
    }


    // Main function to display stealth
    //  Pre: bool to represent whether to turn this on or off
    //  Post: displays stealth if enemy is invisible
    public void displayStealth(bool invisible, float curInvisTimer, float invisDuration) {
        if (invisibilityIcon != null) {
            bool prevInvisible = invisibilityIcon.gameObject.activeInHierarchy;

            // Only change sort order if manic status changed to reduce cost
            if (prevInvisible != invisible) {
                changeSortOrder(invisibilityIcon, invisible);
            }

            invisibilityIcon.gameObject.SetActive(invisible);
            invisibilityBar.setStatus(curInvisTimer, invisDuration);
        }
    }


    // Main function to display healing
    //  Pre: bool to represent whether to turn this on or off
    //  Post: displays healing if enemy is healing
    public void displayHealing(bool healing) {
        if (healingIcon != null) {
            bool prevHealing = healingIcon.gameObject.activeInHierarchy;

            // Only change sort order if manic status changed to reduce cost
            if (prevHealing != healing) {
                changeSortOrder(healingIcon, healing);
            }

            healingIcon.gameObject.SetActive(healing);
        }
    }


    // Main function to clear up everything
    //  Pre: none
    //  Post: clears side effects
    public void clear() {
        if (speedIcon != null) {
            speedIcon.gameObject.SetActive(false);
            speedText.gameObject.SetActive(false);
        }

        if (stunIcon != null) {
            stunIcon.gameObject.SetActive(false);
        }

        if (manicIcon != null) {
            manicIcon.gameObject.SetActive(false);
        }

        if (volatileIcon != null) {
            volatileIcon.gameObject.SetActive(false);
        }

        if (invisibilityIcon != null) {
            invisibilityIcon.gameObject.SetActive(false);
        }

        if (healingIcon != null) {
            healingIcon.gameObject.SetActive(false);
        }

        lock(iconOrderLock) {
            activeStatusEffects.Clear();
        }
    }


    // Main helper function to edit the sort order of the UI display
    //  Pre: icon was not previously active when it was added OR it was active when its being removed, deltaX > 0.0f. The parent component of the image represents where the first icon will go
    //  Post: either adds the icon to the right end of the list OR removes the icon and shifts all icons right of it to the left.
    //  O(n) time on removal and O(1) time on add
    private void changeSortOrder(Image icon, bool added) {
        Debug.Assert(icon != null);
        Debug.Assert(icon.gameObject.activeInHierarchy != added);
        Debug.Assert(deltaX > 0.0f);

        lock (iconOrderLock) {
            // If added, modift the x component of the icon and add it to the list of active effects
            if (added) {
                icon.rectTransform.anchoredPosition = new Vector2(deltaX * activeStatusEffects.Count, 0f);
                activeStatusEffects.Add(icon);

            // If removed, remove the icon and anything that's to the right of it will be shifted left
            } else {
                // Find the index where the icon is found
                int iconIndex = 0;
                while (iconIndex < activeStatusEffects.Count && activeStatusEffects[iconIndex] != icon) {
                    iconIndex++;
                }

                //Move all elements above this index, one index down (and to the left)
                while (iconIndex < activeStatusEffects.Count - 1) {
                    activeStatusEffects[iconIndex] = activeStatusEffects[iconIndex + 1];
                    activeStatusEffects[iconIndex].rectTransform.anchoredPosition = new Vector2(deltaX * iconIndex, 0f);
                    iconIndex++;
                }

                // Remove the last element because that's irrelevant now (if icon was even found)
                if (iconIndex < activeStatusEffects.Count) {
                    activeStatusEffects.RemoveAt(activeStatusEffects.Count - 1);
                }
            }
        }
    }

}
