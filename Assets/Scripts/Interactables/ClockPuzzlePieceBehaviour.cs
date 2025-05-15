using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ClockPuzzlePieceBehaviour : InteractableBehaviourAbstract
{
    public override void OnInteract()
    {
        Debug.Log("puzzle position interacted");
        transform.parent.GetComponent<ClockPuzzleBehaviour>().SetClockInPosition(transform);
    }
}
