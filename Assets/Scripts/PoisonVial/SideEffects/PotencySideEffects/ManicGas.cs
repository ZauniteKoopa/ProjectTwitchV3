using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ManicGas : VirtualSideEffect
{

    [SerializeField]
    [Range(0f, 1f)]
    private float manicIntensity = 0.5f;
    [SerializeField]
    private float manicGasDuration = 9f;
    [SerializeField]
    private float ultCooldown = 15f;
    [SerializeField]
    private int ultCost = 5;
    [SerializeField]
    private LobbingUltimate manicGasPrefab;


    // Main function to check if this is an ultimate
    //  Pre: none
    //  Post: returns the ultimate type for Twitch's Juice. If return NONE, cannot use this as an ultimate
    public override UltimateType getUltType() {
        return UltimateType.LOB;
    }


    // Main function to get ultimate cooldown
    //  Pre: none
    //  Post: cooldown > 0f
    public override float getUltimateCooldown() {
        return ultCooldown;
    }

    // Main function to get ultimate ammo cost
    //  Pre: none
    //  Post: cost > 0f
    public override int getUltimateCost() {
        return ultCost;
    }


    // Main function to throw ult lobs
    //  Pre: startPosition is the start position of the lob, end position is the end position of the lob, statNum is the important stat value
    //  Post: launches lobbing ultimate
    public override void throwLobbingUltimate(Vector3 startPos, Vector3 endPos, int statNum) {
        // Calculate values
        float[] ultParameters = new float[] {manicIntensity, manicGasDuration};

        // Instantiate objects
        LobbingUltimate currentUlt = Object.Instantiate(manicGasPrefab, startPos, Quaternion.identity);
        currentUlt.launch(endPos, ultParameters);
    }


    // Main override function for getting the description
    public override string getDescription() {
        float attackBuff = (1.0f + manicIntensity) * 100f;
        float armorDebuff = (1.0f - manicIntensity) * 100f;
        return "Gain new ultimate: throw a cask that releases gas over an area for " + manicGasDuration + " seconds that induces MANIC on ALL units within the zone. Manic units will armor reduced to " + armorDebuff + "%, but attack buffed by " + attackBuff + "%";
    }
}
