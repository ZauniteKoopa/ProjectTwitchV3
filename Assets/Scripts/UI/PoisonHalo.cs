using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonHalo : INumberDisplay
{
    // Sprites to activate
    [SerializeField]
    private SpriteRenderer[] poisonSprites;
    [SerializeField]
    private SpriteRenderer bigPoisonSprite;


    // Main function to display a positive number, whether through icons or text
    //  Pre: number is the value you want to display
    //  Post: displays value in whatever way the display wants
    public override void displayNumber(int number) {
        // If number is less than length of poison sprites: just turn on the number of poison sprites
        if (number <= poisonSprites.Length) {

            for (int i = 0; i < poisonSprites.Length; i++) {
                // Activate all the ones encompassed by number (number > i)
                poisonSprites[i].enabled = (i < number);
            }

            bigPoisonSprite.enabled = false;

        // Else, deactivate all sprites and only activate the big one
        } else {
            foreach (SpriteRenderer poisonSprite in poisonSprites) {
                poisonSprite.enabled = false;
            }

            bigPoisonSprite.enabled = true;
        }
    }


    // Main function to change the color of this INumberDisplay
    //  Pre: color is the color you want to change to
    //  Post: updates color accordingly
    public override void displayColor(Color color) {
        bigPoisonSprite.color = color;

        foreach (SpriteRenderer poisonSprite in poisonSprites) {
            poisonSprite.color = color;
        }
    }
}
