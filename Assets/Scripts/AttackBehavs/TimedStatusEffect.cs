using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;

// Main function to handle timed status effects (specifically priority queues)
public class TimedStatusEffect : IComparable<TimedStatusEffect>
{
    // The two main variables
    private float endTime;
    public Coroutine statusEffectSequence;


    // Main constructor
    public TimedStatusEffect(float effectDuration, Coroutine sequence) {
        Debug.Assert(effectDuration >= 0.0f && sequence != null);

        endTime = Time.fixedTime + effectDuration;
        statusEffectSequence = sequence;
    }


    // Main function to compare timed status effect
    public int CompareTo(TimedStatusEffect other) {
        return endTime.CompareTo(other.endTime);
    }

}
