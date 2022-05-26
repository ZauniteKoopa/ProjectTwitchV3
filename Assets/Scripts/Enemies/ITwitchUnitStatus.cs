using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ITwitchUnitStatus : IUnitStatus
{
    // Main method to do poison damage (specific to twitch damage)
    //  initDmg: initial, immediate damage applied to enemy, > 0
    //  poison: PoisonVial that will be inflicted to this enemy.
    //  numStacks: number of stacks applied to enemy when doing immediate damage
    //  Post: damage AND poison will be applied to enemy
    public abstract void poisonDamage(float initDmg, IVial poison, int numStacks);


    // Main method to do poison damage (specific to twitch damage)
    //  initDmg: initial, immediate damage applied to enemy, > 0
    //  poison: PoisonVial that will be inflicted to this enemy IFF unit isn't already inflicted with poison
    //  numStacks: number of stacks applied to enemy when doing immediate damage
    //  Post: damage AND poison will be applied to enemy
    public abstract void weakPoisonDamage(float initDmg, IVial poison, int numStacks);
}