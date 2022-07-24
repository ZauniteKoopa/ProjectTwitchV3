using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ExplosiveSurprise : VirtualSideEffect
{
    [SerializeField]
    private float baseExplosionDamage = 5f;
    [SerializeField]
    private float explosiveDamageGrowth = 3f;
    [SerializeField]
    private float speedReduction = 0.6f;
    [SerializeField]
    private float slowDuration = 1.5f;


    // Main function to check if this is a player aura side effect (Player)
    public override bool isPlayerAuraEffect() {
        return true;
    }

    // Main function to execute enemy aura with consideration of aura type
    //  Pre: aura != null, vial != null, 0 <= numStacks <= 6
    public override void executeAuraDamage(EnemyAura aura, AuraType auraType, int numStacks, IVial vial) {
        Debug.Assert(aura != null && vial != null && 0 <= numStacks && numStacks <= 6);

        if (auraType == AuraType.SURPRISE) {
            // Do explosive damage
            float explosiveDmg = Mathf.Max(baseExplosionDamage + (explosiveDamageGrowth * (numStacks - 3)), baseExplosionDamage);
            aura.damageAllTargets(explosiveDmg);

            // Do slow status
            aura.applyTimedSlowEffect(speedReduction, slowDuration);
        }
    }


    // Main override function for getting the description
    public override string getDescription() {
        float slowPercentage = (1.0f - speedReduction) * 100f;
        return "When popping out of stealth, Twitch will do AOE damage around him (scaling with the vial's reactivity stat), slowing enemies by " + slowPercentage + "% for " + slowDuration + " seconds";
    }
}
