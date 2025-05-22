using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClockPuzzlePieceBehaviour : InteractableBehaviourAbstract
{
    private ClockPuzzleBehaviour clockHand;
    public override void OnInteract()
    {
        // Debug.Log("puzzle position interacted");
        clockHand.SetClockInPosition(transform);
    }

    public void SetClockHand(ClockPuzzleBehaviour pClockHand)
    {
        clockHand = pClockHand;
    }
}
